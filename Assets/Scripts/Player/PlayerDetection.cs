using Game.Scripts.Gameplay;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private PlayerController playerController;
    public bool IsPlaying = false;
    private IEnemy currentEnemy;

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
        if (IsPlaying) return;
        Collider2D[] hits = GetEnemies();

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<IEnemy>();
            if (enemy != null)
            {
                if (enemy.IsWin) continue;
                else
                {
                    currentEnemy = enemy;
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

    private void StartPlay()
    {
        currentEnemy?.OnPlayerRequest(playerController);
    }
    public void OnPlayerWin()
    {
        IsPlaying = false;
        playerController.UnStop();
        currentEnemy.OnWinning();
    }
    public void OnPayerLose()
    {
        IsPlaying = false;
        currentEnemy.OnPlayerMissed();
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