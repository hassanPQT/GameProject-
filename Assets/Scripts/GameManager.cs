using Game.Scripts.Gameplay;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController Player;
    public SongWheelController songWheelController;

    [SerializeField] private MoodBarController moodBar;
    [SerializeField] private float moodDeltaOnWin = 0.15f;
    [SerializeField] private float moodDeltaOnLose = 0.15f;
    [SerializeField] private float _inputTimeout = 10f;
    [SerializeField] private float timerSpeed = 0.5f; // Thêm dòng này vào class


    private bool _awaitingPlayerSelect;
    private SongDirection[] _targetDir;
    private int _userPositivePoint = 3;
    private bool _isGameEnd;
    private bool _isGamePaused;

    private List<int> _directionNumberList;
    public float Timer;
    public bool IsWinToStopEnemy;
    public bool IsInputEnable;
    public bool IsStop3s => _awaitingPlayerSelect;
    public float ModifyTimeout = 1;
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

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsWinToStopEnemy = false;
        IsInputEnable = true;
        _currentDirectionIndex = 6;
        _userPositivePoint = 3;
        SetupDirectionNumbers();
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

    private void Update()
    {
        if (_isGamePaused || _isGameEnd) return;
        HandleKeyboardInput();
    }

    public void OnEnemySignal(SongDirection[] dir, IEnemy enemy)
    {
        _targetDir = dir;
        StartCoroutine(WaitForInput(enemy));
    }

    private IEnumerator WaitForInput(IEnemy enemy)
    {
        _awaitingPlayerSelect = true;
        Timer = 0f;
        var temp = _inputTimeout * ModifyTimeout;
        while (Timer < temp)
        {
            Timer += Time.deltaTime ;
            yield return null;
        }

        // het count down ma player chua select
        if (_awaitingPlayerSelect)
        {
            OnPlayerResult(false);
            _awaitingPlayerSelect = false;
        }
    }

    public bool OnSongWheelSelect(int[] sliceIndex)
    {
        if (!_awaitingPlayerSelect) return false;

        bool correct = sliceIndex.Length == 2 && _targetDir != null && sliceIndex.Length == _targetDir.Length;
        if (correct)
        {
            for (int i = 0; i < sliceIndex.Length; i++)
            {
                if (sliceIndex[i] != (int)_targetDir[i])
                {
                    correct = false;
                    break;
                }
            }
        }


        OnPlayerResult(correct);
        _awaitingPlayerSelect = false;

        return correct;
    }

    private void OnPlayerResult(bool success)
    {
        if (success)
        {
            OnPlayerWin();
        }
        else
        {
            OnPlayerLose();
        }
    }

    private void OnPlayerLose()
    {
        Player.OnPayerLose();
        songWheelController.OnPlayerLose();
        _awaitingPlayerSelect = false;
        IsInputEnable = true;
        OnPlayerLoseEncounter();
    }

    private void OnPlayerWin()
    {
        Debug.Log("Đúng! Enemy bị cảm hóa.");
        Player.OnPlayerWin();
        songWheelController.OnPlayerWin();
        _awaitingPlayerSelect = false;
        IsWinToStopEnemy = true;
        IsInputEnable = true;
        OnPlayerWinEncounter();
    }

    private void HandleKeyboardInput()
    {
        Player.Run(Input.GetKey(KeyCode.LeftShift));
        if (!IsInputEnable)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector2 input = new Vector2(horizontal, 0f);
        Player.Move(input);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            Player.Jump();
    }

    public void TakeDamage(int i)
    {
        _userPositivePoint--;
        if (_userPositivePoint == 0)
            ShowFailGame();
    }

    public void ShowFailGame()
    {
        if (_isGameEnd) return;
        _isGameEnd = true;
        PauseGame();
    }

    public void PauseGame()
    {
        _isGamePaused = true;
    }
    public void ResumeGame() => _isGamePaused = false;
}
