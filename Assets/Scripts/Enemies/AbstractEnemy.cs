using Game.Scripts.Gameplay;
using System;
using UnityEngine;

public class AbstractEnemy : MonoBehaviour, IEnemy, IListener
{

    [SerializeField] protected float _moveDistance = 2f;
    [SerializeField] protected float _moveDuration = 0.3f;
    [SerializeField] protected float _moveSpeed = 5f;
    [SerializeField] protected GameObject happyMood;
    [SerializeField] protected GameObject angryMood;

    [SerializeField] protected EnemySignal signal;
    [SerializeField] protected bool _isMoving = true;
    protected Vector3 _startPoint;
    public Action<SongDirection[]> Singal;
    protected bool _hasDetected;

    protected virtual void Start()
    {
        _hasDetected = false;
        _startPoint = transform.position;
        IsWin = false;
    }
    public bool IsWin { get ; set ; }
    public bool IsMoving { get => _isMoving; set => _isMoving = value; }

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
        if (_hasDetected) return;
        float detectRadius = 4f;
        LayerMask playerLayer = LayerMask.GetMask("Player");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                _hasDetected = true;
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

    public virtual void Play()
    {
        var direction = signal.SignalRandomDirection(_moveDistance, _moveDuration);
        if (direction != null)
        {
            Singal?.Invoke(direction);
        }
    }

    public void Playing()
    {
    }

    public void Pause()
    {
    }

    public void GameWin()
    {
    }

    public void GameLose()
    {
    }
}
