using UnityEngine;
using System;
using System.Collections;

public enum SongDirection
{
    Up = 0,
    UpRight = 7,
    Right = 6,
    DownRight = 5,
    Down = 4,
    DownLeft = 3,
    Left = 2,
    UpLeft = 1
}

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveDuration = 0.2f;

    public event Action<SongDirection[]> OnSignalDirection;

    private Vector3 _leftPos, _rightPos, _startPos;
    private bool _isMoving = true;
    private bool _isMovingInDirection = false;
    private SongDirection[] _currentDir;

    private void Start()
    {
        SetPositionToMove();
        StartCoroutine(MoveBackAndForth());
    }

    private void SetPositionToMove()
    {
        _startPos = transform.position;
        _leftPos = _startPos + Vector3.left * moveDistance;
        _rightPos = _startPos + Vector3.right * moveDistance;
    }

    private void Update()
    {
        if (!_isMovingInDirection)
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
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            elapsed += Time.deltaTime / 1.5f;
            yield return null;
        }
        transform.position = target;
    }

    private void MoveBackToFirstPosition()
    {
        _isMovingInDirection = true;
        Vector3 start = transform.position;
        Vector3 end = _startPos;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime / 1.5f;
        }
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
            _currentDir[i] = (SongDirection)UnityEngine.Random.Range(0, 7);

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
            Vector3 dirVec = DirectionToVector(d);
            Vector3 start = transform.position;
            Vector3 end = start + dirVec * moveDistance;

            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, t / moveDuration);
                yield return null;
            }

            t = 0;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(end, start, t / moveDuration);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(10f);
        _isMovingInDirection = false;
    }

    private Vector3 DirectionToVector(SongDirection dir)
    {
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }
}
