using DG.Tweening;
using Game.Scripts.Gameplay;
using System;
using System.Collections;
using UnityEngine;

public abstract class AbstractEnemy : MonoBehaviour
{
    [SerializeField] protected float _moveDistance = 2f;
    [SerializeField] protected float _moveDuration = 0.3f;
    [SerializeField] protected float _moveSpeed = 5f;
    [SerializeField] protected GameObject happyMood;
    [SerializeField] protected GameObject angryMood;

    [SerializeField] protected EnemySignal signal;
    [SerializeField] protected bool _isMoving = true;
    public Vector3 _startPoint;
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
                Debug.Log("Detected player: " + hit.name);
                _hasDetected = true;
                OnDetectPlayer(hit.GetComponent<PlayerController>());
                return;
            }
        }
    }
    public virtual void OnDetectPlayer(PlayerController player)
    {
    }

    public abstract IEnumerator OnPlayerMissed();

    public abstract IEnumerator OnPlayerRequest(PlayerController playerController);

    public virtual void OnWinning()
    {
        StopAllCoroutines();
    }

    public virtual IEnumerator Play()
    {
        Debug.Log("Playyyyy");
        transform.DOKill();
        if (transform.position.y - _startPoint.y > 3)
        {
            yield return  transform.DOMove(_startPoint, 0.2f).SetEase(Ease.Linear);
        }
        var direction = signal.GetSongDirection();
        if (direction != null)
        {
            Debug.Log("Playyyyy on signal");

            Singal?.Invoke(direction);
            StartCoroutine(signal.SignalRandomDirection(_moveDistance, _moveDuration)); 
        }
    }
}
