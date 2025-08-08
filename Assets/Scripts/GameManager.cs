using Game.Scripts.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController Player;
    [SerializeField] private SongWheelController _songWheelController;

    [SerializeField] private MoodBarController moodBar;
    [SerializeField] private float moodDeltaOnWin = 0.15f;
    [SerializeField] private float moodDeltaOnLose = 0.15f;
    [SerializeField] private float _inputTimeout = 10f;

    private bool _awaitingInput;
    private SongDirection[] _targetDir;
    private int _userPositivePoint = 3;
    private bool _isGameEnd;
    private bool _isGamePaused;

    public int[] DirectionNumber;
    public bool IsWin;
    public float Timer;
    public bool IsWinToStopEnemy;
    public bool IsInputEnable;
    public bool IsStop3s => _awaitingInput;
    public float ModifyTimeout = 1;

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
        IsWin = false;
        _userPositivePoint = 3;
        SetupDirectionNumbers();
    }

    private void SetupDirectionNumbers()
    {
        DirectionNumber = new int[2];
        DirectionNumber[0] = 0;
        DirectionNumber[1] = 7;
    }

    private void Update()
    {
        if (_isGamePaused || _isGameEnd) return;
        HandleKeyboardInput();
    }

    public void OnEnemySignal(SongDirection[] dir)
    {
        _targetDir = dir;
        _awaitingInput = true;
        StartCoroutine(WaitForInput());
    }

    private IEnumerator WaitForInput()
    {
        Timer = 0f;
        var temp = _inputTimeout * ModifyTimeout;
        while (_awaitingInput && Timer < temp)
        {
            Timer += Time.deltaTime;
            yield return null;
        }

        if (_awaitingInput)
        {
            OnPlayerResult(false);
        }
    }

    public void OnPlayerSelect(int[] sliceIndex)
    {
        if (!_awaitingInput) return;//cẩn thận chỗ này

        _awaitingInput = false;
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
        OnPlayerResult( correct);
    }

    private void OnPlayerResult(bool success)
    {
        if (success)
        {
            Debug.Log("Đúng! Enemy bị cảm hóa.");
            IsWin = true;
            IsWinToStopEnemy = true;
            IsInputEnable = true;
            OnPlayerWinEncounter();
        }
        else
        {
            IsWin = false;
            Player.IsSignaling = false;
            IsInputEnable = true;
            OnPlayerLoseEncounter();
            Debug.Log("Sai! Bị trượt.");
        }
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
        Player.Stop();
        _isGamePaused = true;
    }

    public void LostGame()
    {

    }
    public void ResumeGame() => _isGamePaused = false;
}
