using Game.Scripts.Gameplay;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    private int _userPositivePoint = 3;
    private bool _isGameEnd;
    private bool _isGamePaused;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _userPositivePoint = 3;
    }

    private void Update()
    {
        if (_isGamePaused) return;
        if (_isGameEnd) return;

        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D hoặc ←/→
        Vector2 input = new Vector2(horizontal, 0f);
        player.Move(input);

        // Nhảy với Space hoặc W
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            player.Jump();
        }
    }

    public void TakeDamage(int i)
    {
        _userPositivePoint--;
        if (_userPositivePoint == 0)
        {
            ShowFailGame();
        }
    }

    public void ShowFailGame()
    {
        if (_isGameEnd) return;
        _isGameEnd = true;
        PauseGame();
    }

    public void PauseGame()
    {
        player.Stop();
        _isGamePaused = true;
    }

    public void ResumeGame() => _isGamePaused = false;
}
