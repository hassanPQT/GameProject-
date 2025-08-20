using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;


public class EnemyController : AbstractEnemy
{
    private Vector3 _leftPos, _rightPos, _startPos;
    private bool _isMovingInDirection = false;
    public bool IsMovingInDirection => _isMovingInDirection;
    [SerializeField] private AudioClip enemySfx;
    protected override void Start()
    {
        base.Start();
        _hasDetected = false;
        SetPositionToMove();
        StartCoroutine(MoveBackAndForth());
        SetAngryMood(true);
    }
    public void ResetEnemy()
    {
        _isMoving = true;
        SetPositionToMove();
        StartCoroutine(MoveBackAndForth());
    }

    private void SetPositionToMove()
    {
        _startPos = transform.position;
        _leftPos = _startPos + Vector3.left * _moveDistance;
        _rightPos = _startPos + Vector3.right * _moveDistance;
    }

    private void Update()
    {
        DetectPlayer();
    }

    private IEnumerator MoveBackAndForth()
    {
        while (!_hasDetected)
        {
            yield return MoveToPosition(_leftPos);
            yield return MoveToPosition(_rightPos);
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 start = transform.position;
        transform.DOMove(target, _moveDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(_moveDuration);
    }

    private void MoveBackToFirstPosition()
    {
        StartCoroutine(_MoveBackCoroutine());
    }

    private IEnumerator _MoveBackCoroutine()
    {
        _isMovingInDirection = true;

        Vector3 end = new Vector3(_startPos.x + 5f, _startPos.y, _startPos.z);
        transform.DOMove(end, _moveDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(_moveDuration); 
        _isMovingInDirection = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5);
    }


    public void StopMovement()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider) collider.enabled = false;
        enabled = false;
    }

    public void MakeMovement()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider) collider.enabled = true;
        enabled = true;
    }

    public override void OnDetectPlayer(PlayerController playerController)
    {
        _isMoving = false;
        StopCoroutine(MoveBackAndForth());
        MoveBackToFirstPosition();
    }

    public override IEnumerator OnPlayerRequest(PlayerController playerController)
    {
        //playerController.movement.StopPlayer();
        yield return Play();
    }

    public override void OnWinning()
    {
        GameManager.Instance.OnPlayerWinEncounter();
        StopMovement();
        SetAngryMood(false);
        SetActiveMood(true);
    }
    public override IEnumerator OnPlayerMissed()
    {
        _isMoving = false;
        GameManager.Instance.OnPlayerLoseEncounter();
        yield return ShakeBird();
    }

    //private IEnumerator WaitPlay()
    //{
    //    yield return new WaitForSeconds(1f);
    //    yield return Play();
    //}

    

    private IEnumerator ShakeBird()
    {
        //StopAllCoroutines();
        // Shake parameters
        float duration = 1f;
        float strength = 0.2f;
        int vibrato = 20;

        // Use DOTween's DOShakePosition for shaking effect
        yield return transform.DOShakePosition(duration, strength, vibrato)
            .SetEase(Ease.Linear).WaitForCompletion();
        AudioManager.Instance.PlaySFX(enemySfx);

        yield return Play();
    }
}
