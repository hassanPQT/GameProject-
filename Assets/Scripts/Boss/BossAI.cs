using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    private static readonly int IsDie = Animator.StringToHash("isDie");
    private static readonly int IsAttack = Animator.StringToHash("isAttack");

    [SerializeField] private Animator animator;
    [SerializeField] private Transform left;
    [SerializeField] private Transform leftUP;
    [SerializeField] private Transform leftDown;
    [SerializeField] private GameObject winUI;

    private bool _isWin;
    private bool _isPlayerInside => Vector3.Distance(transform.parent.position, FindFirstObjectByType<PlayerController>().transform.position) < 25f;
    private Vector2 _currentDir;
    private void Awake()
    {
        left.gameObject.SetActive(false);
        leftUP.gameObject.SetActive(false);
        leftDown.gameObject.SetActive(false);
        _isWin = false;

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        _isWin = true;
        animator.SetBool(IsDie, true);
        animator.SetBool(IsAttack, false);
        yield return new WaitForSeconds(2f);
        InputManager.Instance.GameLose();
        Time.timeScale = 0;
        Debug.Log("You Winn");
        winUI.SetActive(true);
    }

    public IEnumerator EnterPlay()
    {
        Debug.Log("Boss AI is starting");
        while (!_isWin)
        {
            Debug.Log("Boss AI is running");
            if (!_isPlayerInside)
            {
                // push effect tat no di
                yield return null;
                continue;
            }
            Debug.Log("Boss AI is running, player is inside");
            // bat push effct
            var Push = FindFirstObjectByType<PushBackEffect>();
            yield return UpdateUI(GetRandomAttackDir(), Push);
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private readonly Vector2[] attackDir = new Vector2[] {
        Vector2.left,
        Vector2.left + Vector2.up,
        Vector2.left + Vector2.down
    };
    private Vector2 _attackDir;

    private Vector2 GetRandomAttackDir()
    {
        _attackDir = attackDir[Random.Range(0, attackDir.Length)];
        return _attackDir;
    }

    private IEnumerator UpdateUI(Vector2 dir, PushBackEffect backEffect)
    {
        yield return new WaitForSeconds(3);

        if (_currentDir != dir)
        {
            _currentDir = dir;
            // show animation background
            if (dir == Vector2.left)
            {
                UpdateUI(0);
            }
            if (dir == Vector2.left + Vector2.up)
            {
                UpdateUI(1);

            }
            if(dir == Vector2.left + Vector2.down)
            {
                UpdateUI(2);
            }
            backEffect.BossDirection = dir;
        }
    }

    public void UpdateUI(int number)
    {
        left.gameObject.SetActive(false);
        leftUP.gameObject.SetActive(false);
        leftDown.gameObject.SetActive(false);
        switch (number)
        {
            case 0:
                left.gameObject.SetActive(true); break;
            case 1:
                leftUP.gameObject.SetActive(true); break;
            case 2:
                leftDown.gameObject.SetActive(true); break;
            default:
                break;
        }
    }
}
