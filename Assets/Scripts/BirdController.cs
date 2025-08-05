using System;
using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float flySpeed = 10f;
    [SerializeField] private float stayDuration = 10f;
    [SerializeField] private float hoverHeight = 1.5f;

    public event Action<SongDirection[]> OnSignalDirection;

    private SongDirection[] _currentDir;
    private Vector3 _smoothVelocity = Vector3.zero;

    public bool SignalRandomDirection()
    {
        _currentDir = new SongDirection[2];
        for (int i = 0; i < _currentDir.Length; i++)
            _currentDir[i] = (SongDirection)UnityEngine.Random.Range(0, 7);

        OnSignalDirection?.Invoke(_currentDir);
        StartCoroutine(MoveInDirection(_currentDir));
        return true;
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
                t += Time.deltaTime * 1.8f;
                transform.position = Vector3.Lerp(start, end, t / moveDuration);
                yield return null;
            }

            t = 0;
            while (t < moveDuration)
            {
                t += Time.deltaTime * 1.8f;
                transform.position = Vector3.Lerp(end, start, t / moveDuration);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }
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

    private Vector3 DirectionToVector(SongDirection dir)
    {
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }

    public void FlyIntoPlayer()
    {
        StartCoroutine(FlyCoroutine());
    }

    private IEnumerator FlyCoroutine()
    {
        Vector3 offset = Vector3.up * hoverHeight;
        var player = GameManager.Instance.Player;
        Vector3 targetPos = player.transform.position + offset;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);
            yield return null;
        }

        float elapsed = 0f;
        while (elapsed < stayDuration)
        {
            targetPos = player.transform.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _smoothVelocity, 0.15f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        OnStayFinished();
    }

    private void OnStayFinished()
    {
        Debug.Log("Bird stay time ended.");
        // Add next logic here
    }
}
