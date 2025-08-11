using Game.Scripts.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState
{


}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController Player;
    public SongWheelController songWheelController;


    [SerializeField] private MoodBarController moodBar;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Transform PlayerPrf;
    [SerializeField] private Transform startPoint;
    [SerializeField] private float moodDeltaOnWin = 0.15f;
    [SerializeField] private float moodDeltaOnLose = 0.15f;
    [SerializeField] private float _inputTimeout = 10f;


    //public bool _awaitingPlayerSelect;
    //private SongDirection[] _targetDir;
    //private int _userPositivePoint = 3;

    private List<int> _directionNumberList;
    //public float Timer;
    public bool IsInputEnable;
    //public bool IsStop3s => _awaitingPlayerSelect;
    //public float ModifyTimeout = 1;
    public int[] DirectionNumber => _directionNumberList.ToArray();
    private int _currentDirectionIndex;

    private List<IListener> _listenerList = new List<IListener>();
    private void Awake()
    {
        if (Instance == null)
        {
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        InitializedStatus();
        InitializedPlayer();
        SetupDirectionNumbers();
    }
    public SongDirection GetRandomSongDirection()
    {
        return (SongDirection)DirectionNumber[UnityEngine.Random.Range(0, DirectionNumber.Length)];
    }
    private void InitializedPlayer()
    {
        Debug.Log("init player");
        if (PlayerPrf == null)
        {
            Debug.LogError("PlayerPrefab is not assigned in the Inspector!");
            return;
        }

        startPoint = GameObject.FindWithTag("StartPoint").transform;
        
        Debug.Log(startPoint.position.ToString());

        // Nếu không tìm thấy Player, tạo mới
        if (Player == null)
        {
            Player = Instantiate(PlayerPrf, startPoint.position, startPoint.rotation).GetComponent<PlayerController>();
            Debug.Log("Player instantiated at: " + startPoint.position);
        }
        else
        {
            // Nếu Player đã tồn tại, chỉ di chuyển đến startPoint
            Player.transform.position = startPoint.position;
            Player.transform.rotation = startPoint.rotation;
            Debug.Log("Existing Player moved to: " + startPoint.position);
        }
    }

    private void InitializedStatus()
    {
        InputManager.Instance.LockCursor();
        IsInputEnable = true;
        _currentDirectionIndex = 6;
        //_userPositivePoint = 3;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Giả sử Player, Canvas, MoodBar đều có tag hoặc có thể FindObjectOfType
        StopAllCoroutines();
        Player = FindAnyObjectByType<PlayerController>();
        InitializedPlayer();
        InitializedStatus();
        SetupDirectionNumbers();
        uiManager = FindAnyObjectByType<UIManager>();

        songWheelController = FindAnyObjectByType<SongWheelController>();
        moodBar = FindAnyObjectByType<MoodBarController>();
        // Gán thêm nếu cần…
    }

    public void AddListener(IListener listener)
    {
        _listenerList.Add(listener);
    }
    public void ReleaseListener(IListener listener)
    {
        if (_listenerList.Contains(listener)) 
        _listenerList.Remove(listener);
    }
    // khi player win một lượt đấu
    public void OnPlayerWinEncounter()
    {
        moodBar.IncreaseMood(moodDeltaOnWin);
    }

    // khi player lose một lượt đấu
    public void OnPlayerLoseEncounter()
    {
        moodBar.DecreaseMood(moodDeltaOnLose);
    }

    private void SetupDirectionNumbers()
    {
        _directionNumberList = new List<int>();
        _directionNumberList.Add(0);
        _directionNumberList.Add(7);
    }

    public void AddMoreRandomSongWheelNumbers()
    {
        if (_currentDirectionIndex == 0)
            return;
        _directionNumberList.Add(_currentDirectionIndex);
        _currentDirectionIndex--;
    }

    public void PauseGame()
    {
        foreach (var listener in _listenerList)
        {
            listener.Pause();
        }
    }
    public void GameLose()
    {
        InputManager.Instance.GameLose();
        uiManager.ShowLoseUI();
        PauseGame();
        foreach (var listener in _listenerList)
        {
            listener.GameLose();
        }
        //Player.movement.PlayGiveUpAnimation();
    }
    public void ResumeGame()
    {
        
    }

    internal void GameRestart()
    {
        Debug.Log("Restart scen");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
