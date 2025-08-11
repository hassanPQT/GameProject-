using Game.Scripts.Gameplay;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private PlayerController playerController;
    private AbstractEnemy currentEnemy;
    private SongDirection[] _targetDir;
    //public bool IsPlaying = false;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        DetectEnemy();
    }

    private void DetectEnemy()
    {
        if (currentEnemy != null) return;
        Collider2D[] hits = GetEnemies();

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<AbstractEnemy>();
            if (enemy != null)
            {
                if (enemy.IsWin) continue;
                else
                {
                    currentEnemy = enemy;
                    currentEnemy.Singal += x => _targetDir = x;
                    StartPlay();
                }
            }
        }

    }

    private Collider2D[] GetEnemies()
    {
        float detectRadius = 3.5f;
        LayerMask enemyLayer = LayerMask.GetMask("Flying");
        DrawDebugCircle(transform.position, detectRadius, Color.red);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
        return hits;
    }
    public void SelectSongWheel(int[] sliceIndex)
    {
        bool result = CheckCondition(sliceIndex);
        if (result)
        {
            OnPlayerWin();
        }
        else
        {
            OnPayerLose();
        }
    }

    private bool CheckCondition(int[] sliceIndex)
    {
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
        return correct;
    }

    private void StartPlay()
    {
        Debug.Log("start play");
        currentEnemy?.OnPlayerRequest(playerController);
    }
    public void OnPlayerWin()
    {
        //IsPlaying = false;
        playerController.movement.UnStop();
        currentEnemy.OnWinning();
    }
    public void OnPayerLose()
    {
        //IsPlaying = false;
        currentEnemy.OnPlayerMissed();
        currentEnemy.Singal = null;
        currentEnemy = null;
    }
    private void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        for (int i = 1; i <= segments; i++)
        {
            angle += 2 * Mathf.PI / segments;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Debug.DrawLine(lastPoint, nextPoint, color, 0.01f);
            lastPoint = nextPoint;
        }
    }
}