using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySignal enemySignal;
    private bool IsPlaying = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlaying) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            IsPlaying = true;
            StartCoroutine(enemySignal.SignalRandomDirection(1,1));
        }
    }
    
}
