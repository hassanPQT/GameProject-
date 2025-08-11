using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EnemySignal : MonoBehaviour
{

    [Header("Signal Effect")]
    [SerializeField] private GameObject[] signalEffectPrefab;
    [SerializeField] private float _effectDuration = 0.5f;
    [SerializeField] private float _effectMaxScale = 1.5f;
    [SerializeField] public bool _isMoving = true;
    private IEnemy m_Enemy;
    private SongDirection[] _currentDir;
    public bool SignalRandomDirection( float moveDistance, float moveDuration)
    {
        if (_isMoving) return true;
        Debug.Log("`  SignalRandomDirection` called in BirdController.");

        _currentDir = new SongDirection[2];
        for (int i = 0; i < _currentDir.Length; i++)
            _currentDir[i] = (SongDirection)GameManager.Instance.DirectionNumber[UnityEngine.Random.Range(0, GameManager.Instance.DirectionNumber.Length)];

        GameManager.Instance.OnEnemySignal(_currentDir);
        StartCoroutine(MoveInDirection(_currentDir, moveDistance, moveDuration));
        return true;
    }
    private Vector3 DirectionToVector(SongDirection dir)
    {
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }
    private IEnumerator MoveInDirection(SongDirection[] dir, float moveDistance, float moveDuration)
    {
        _isMoving = true;
        yield return new WaitForSeconds(2f);
        foreach (var d in dir)
        {
            ShowSignalEffect(d, moveDistance);
            yield return new WaitForSeconds(_effectDuration * 0.5f);

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
        _isMoving = false;
    }
    private void ShowSignalEffect(SongDirection dir, float moveDistance)
    {
        if (signalEffectPrefab == null || signalEffectPrefab.Length == 0) return;

        Vector3 spawnPos = transform.position + DirectionToVector(dir) * (moveDistance + 1);
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

}
