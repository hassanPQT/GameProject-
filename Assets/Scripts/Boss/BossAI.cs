using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
   [SerializeField]  private Transform left;
   [SerializeField]  private Transform leftUP;

    private Vector2 _currentDir;
    private void Awake()
    {
        left.gameObject.SetActive(false);
        leftUP.gameObject.SetActive(false);

        BossTrigger.OnEnter += () => StartCoroutine(EnterPlay());
        BossTrigger.OnExit += () =>  StopCoroutine(EnterPlay());
    }
    private IEnumerator EnterPlay()
    {
        while (true)
        {
            var Push = FindFirstObjectByType<PushBackEffect>();
            yield return UpdateUI(GetRandomAttackDir(),Push);
        }
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private readonly Vector2[] attackDir = new Vector2[] {
        Vector2.left, 
        Vector2.left + Vector2.up,
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
            backEffect.BossDirection = dir;
        }
    }

    public void UpdateUI(int number)
    {
        left.gameObject.SetActive(false);
        leftUP.gameObject.SetActive(false);
        switch (number)
        {
            case 0:
                left.gameObject.SetActive(true); break;
            case 1:
                leftUP.gameObject.SetActive(true); break;
           
            default:
                break;
        }
    }
}
