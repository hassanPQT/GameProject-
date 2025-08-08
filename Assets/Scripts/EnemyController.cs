using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using Unity.Mathematics;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float _moveDistance = 2f;
    [SerializeField] private float _moveDuration = 0.5f;
    [SerializeField] private float _moveDurationForSongWheel = 0.25f;
    [SerializeField] private float _moveDistanceForSongWheel = 1f;
	[SerializeField] private GameObject happyMood;
    [SerializeField] private GameObject angryMood;

	[Header("Signal Effect")]
    [SerializeField] private GameObject[] signalEffectPrefab;
    [SerializeField] private float _effectDuration = 0.5f;
    [SerializeField] private float _effectMaxScale = 1.5f;

    public event Action<SongDirection[]> OnSignalDirection;

    private Vector3 _leftPos, _rightPos, _startPos;
    private bool _isMoving = true;
    private bool _isMovingInDirection = false;
    private bool _hasDetectedPlayer = false;
    private SongDirection[] _currentDir;

    private void Start()
    {
        SetPositionToMove();
        StartCoroutine(MoveBackAndForth());
        SetAngryMood();
	}
    public void SetAngryMood()
    {
        if (angryMood != null)
        {
            angryMood.SetActive(true);
        }
    }
    public void SetInactiveAngryMood()
    {
        if (angryMood != null)
        {
            angryMood.SetActive(false);
        }
	}
	public void SetActiveMood() { 
        if (happyMood != null)
        {
            happyMood.SetActive(true);
        }
	}
    public void SetInactiveMood() { 
        if (happyMood != null)
        {
            happyMood.SetActive(false);
        }
    }
    public void ResetEnemy()
    {
        _isMoving = true;
        _hasDetectedPlayer = false;
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
        if (!_isMovingInDirection && !_hasDetectedPlayer)
            DetectPlayer();
    }

    private IEnumerator MoveBackAndForth()
    {
        while (_isMoving)
        {
            yield return MoveToPosition(_leftPos);
            yield return MoveToPosition(_rightPos);
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < _moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / _moveDuration);
            elapsed += Time.deltaTime / 1.5f;
            yield return null;
        }
        transform.position = target;
    }

    private void MoveBackToFirstPosition()
    {
        _isMovingInDirection = true;
        StartCoroutine(_MoveBackCoroutine());
    }

    private IEnumerator _MoveBackCoroutine()
    {
        Vector3 start = transform.position;
        Vector3 end = _startPos;
        float elapsed = 0f;
        while (elapsed < _moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / _moveDuration);
            elapsed += Time.deltaTime / 1.5f;
            yield return null;
        }
        transform.position = end;
    }

    private void DetectPlayer()
    {
        float detectRadius = 5.5f;
        LayerMask playerLayer = LayerMask.GetMask("Player");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                _isMoving = false;
                StopCoroutine(MoveBackAndForth());
                MoveBackToFirstPosition();
                _hasDetectedPlayer = true;
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5);
    }

    public bool SignalRandomDirection()
    {
        if (!enabled)
        {
            Debug.LogWarning("EnemyController is not enabled. Cannot signal random direction.");
            return false;
        }

        _currentDir = new SongDirection[2];
        for (int i = 0; i < _currentDir.Length; i++)
            _currentDir[i] = (SongDirection)GameManager.Instance.DirectionNumber[UnityEngine.Random.Range(0,GameManager.Instance.DirectionNumber.Length)];

        OnSignalDirection?.Invoke(_currentDir);

        _isMovingInDirection = true;
        StartCoroutine(MoveInDirection(_currentDir));
        return true;
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

    private IEnumerator MoveInDirection(SongDirection[] dir)
    {
        yield return new WaitForSeconds(2f);

        foreach (var d in dir)
        {
            // Hiệu ứng signal trước khi di chuyển
            ShowSignalEffect(d);
            yield return new WaitForSeconds(_effectDuration * 0.5f);

            Vector3 dirVec = DirectionToVector(d);
            Vector3 start = transform.position;
            Vector3 end = start + dirVec * _moveDistanceForSongWheel;

            float t = 0f;
            while (t < _moveDurationForSongWheel)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, t / _moveDurationForSongWheel);
                yield return null;
            }

            t = 0;
            while (t < _moveDurationForSongWheel)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(end, start, t / _moveDurationForSongWheel);
                yield return null;
            }
        }

        yield return new WaitForSeconds(10f);
        _isMovingInDirection = false;
    }

    private void ShowSignalEffect(SongDirection dir)
    {
        if (signalEffectPrefab == null || signalEffectPrefab.Length == 0) return;

        Vector3 spawnPos = transform.position + DirectionToVector(dir) * (_moveDistanceForSongWheel + 1);
        GameObject fx = Instantiate(signalEffectPrefab[UnityEngine.Random.Range(0, signalEffectPrefab.Length)], spawnPos, Quaternion.identity);

        // Reset scale and alpha
        fx.transform.localScale = Vector3.zero;
        var sr = fx.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 1f);

        // Tween pop-in and fade-out
        Sequence seq = DOTween.Sequence();
        seq.Append(fx.transform.DOScale(_effectMaxScale, _effectDuration * 0.4f).SetEase(Ease.OutBack));
        seq.Append(fx.transform.DOScale(_effectMaxScale * 1.2f, _effectDuration * 0.2f).SetEase(Ease.Linear));
        seq.Join(sr.DOFade(0f, _effectDuration).SetEase(Ease.Linear));
        seq.OnComplete(() => Destroy(fx));
    }

    private Vector3 DirectionToVector(SongDirection dir)
    {
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }

  
}
