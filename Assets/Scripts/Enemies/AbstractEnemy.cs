using Game.Scripts.Gameplay;
using UnityEngine;

public class AbstractEnemy : MonoBehaviour, IEnemy
{

    [SerializeField] protected float _moveDistance = 2f;
    [SerializeField] protected float _moveDuration = 0.3f;
    [SerializeField] protected float _moveSpeed = 5f;
    [SerializeField] protected GameObject happyMood;
    [SerializeField] protected GameObject angryMood;

    [SerializeField] protected EnemySignal signal;
    [SerializeField] protected bool _isMoving = true;
    protected Vector3 _startPoint;

    protected virtual void Start()
    {
        _startPoint = transform.position;
        IsWin = false;
    }
    public bool IsWin { get ; set ; }
    public void SetAngryMood(bool value)
    {
        if (angryMood != null)
        {
            angryMood.SetActive(value);
        }
    }
    protected void SetActiveMood(bool value)
    {
        if (happyMood != null)
        {
            happyMood.SetActive(value);
        }
    }

    protected void SetUnHappyMood(bool value)
    {
        if (angryMood != null)
        {
            angryMood.SetActive(value);
        }
    }

    protected void DetectPlayer()
    {

        float detectRadius = 4f;
        LayerMask playerLayer = LayerMask.GetMask("Player");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                OnDetectPlayer(hit.GetComponent<PlayerController>());
                return;
            }
        }
    }
    public virtual void OnDetectPlayer(PlayerController player)
    {
    }

    public virtual void OnPlayerMissed()
    {
    }

    public virtual void OnPlayerRequest(PlayerController playerController)
    {
    }

    public virtual void OnWinning()
    {
    }
}
