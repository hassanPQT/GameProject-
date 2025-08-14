using Game.Scripts.Gameplay;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossAI : MonoBehaviour
{
   [SerializeField]  private Transform left;
   [SerializeField]  private Transform leftUP;
   [SerializeField]  private Transform leftDown;

    private Vector2 _currentDir;
    private void Awake()
    {
        left.gameObject.SetActive(false);
        leftUP.gameObject.SetActive(false);
        leftDown.gameObject.SetActive(false);

        BossTrigger.OnEnter += () => _playing = true;
        BossTrigger.OnExit += () => _playing = false;
    }
    private bool _playing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private readonly Vector2[] attackDir = new Vector2[] {
        Vector2.left, 
        Vector2.left + Vector2.up,
        Vector2.left + Vector2.down,
    };
    private Vector2 _attackDir;

    public void StartGameLoop(PlayerController controller)
    {
        var pushBackEffect = controller.AddComponent<PushBackEffect>();
        StartCoroutine(GameLoop(controller, pushBackEffect));
    }
    public IEnumerator GameLoop(PlayerController playerController, PushBackEffect backEffect)
    {
        while (_playing)
        {
            yield return PushBackPlayer(playerController, GetRandomAttackDir(), backEffect);
        }
    }
    private Vector2 GetRandomAttackDir()
    {
        _attackDir = attackDir[Random.Range(0, attackDir.Length)];
        return _attackDir;
    }

    private IEnumerator PushBackPlayer(PlayerController player, Vector2 dir, PushBackEffect backEffect)
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
            if (dir == Vector2.left + Vector2.down)
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
