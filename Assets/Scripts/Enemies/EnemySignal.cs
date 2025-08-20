using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EnemySignal : MonoBehaviour
{
    [Header("Signal Effect")]
    [SerializeField] private GameObject signalEffectPrefab;
    [SerializeField] private float _effectDuration = 0.5f;
    [SerializeField] private float _effectMaxScale = 1.5f;
    public SongDirection[] _currentDir;
  
    public SongDirection[] GetSongDirection()
    {
        _currentDir = new SongDirection[2];
        for (int i = 0; i < _currentDir.Length; i++)
            _currentDir[i] = SongWheelManager.Instance.GetRandomSongDirection();
        return _currentDir;
    }
    public IEnumerator SignalRandomDirection(float moveDistance, float moveDuration)
    {
        Debug.Log("enemy move");
        yield return (MoveInDirection(_currentDir, moveDistance, moveDuration));
    }
    private Vector3 DirectionToVector(SongDirection dir)
    {
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }
    private IEnumerator MoveInDirection(SongDirection[] dir, float moveDistance, float moveDuration)
    {        
        yield return new WaitForSeconds(1);
        foreach (var d in dir)
        {
            Debug.Log("SHow signal");
            ShowSignalEffect(d, moveDistance);
            yield return new WaitForSeconds(_effectDuration * 0.5f);

            Vector3 dirVec = DirectionToVector(d);
            Vector3 start = transform.position;
            Vector3 end = start + dirVec * moveDistance;

            Sequence move = DOTween.Sequence();

            move.Append(transform.DOMove(end, moveDuration/2).SetEase(Ease.OutSine));
            move.Append(transform.DOMove(start, moveDuration/2).SetEase(Ease.OutSine));

            yield return move.WaitForCompletion();
        }
    }
    private void ShowSignalEffect(SongDirection dir, float moveDistance)
    {
        if (signalEffectPrefab == null) return;

        // Calculate spawn position
        Vector3 spawnPos = transform.position + DirectionToVector(dir) * (moveDistance + 1);

        // Calculate rotation angle based on SongDirection
        float angleDeg = (int)dir * 45f - 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angleDeg);

        // Instantiate with rotation
        GameObject fx = Instantiate(signalEffectPrefab, spawnPos, rotation);

        // Reset scale and alpha
        fx.transform.localScale = Vector3.zero;
        var sr = fx.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = GetColorByDirection(dir);

        // Tween pop-in and fade-out
        Sequence seq = DOTween.Sequence();
        seq.Append(fx.transform.DOScale(_effectMaxScale, _effectDuration * 0.4f).SetEase(Ease.OutBack));
        seq.Append(fx.transform.DOScale(_effectMaxScale * 1.2f, _effectDuration * 0.2f).SetEase(Ease.Linear));
        seq.Join(sr.DOFade(0f, _effectDuration).SetEase(Ease.Linear));
        seq.OnComplete(() => Destroy(fx));
    }

    private Color GetColorByDirection(SongDirection dir)
    {
        string hex;
        switch (dir)
        {
            case SongDirection.Up: hex = "#138a94"; break;      // Xanh
            case SongDirection.UpRight: hex = "#6f4cdc"; break;      // Xanh ngọc
            case SongDirection.Right: hex = "#d44aca"; break;      // Xanh dương
            case SongDirection.DownRight: hex = "#704edd"; break;      // Tím
            case SongDirection.Down: hex = "#09afde"; break;      // Đỏ
            case SongDirection.DownLeft: hex = "#60d4a0"; break;      // Vàng
            case SongDirection.Left: hex = "#85ce41"; break;      // Trắng xám
            case SongDirection.UpLeft: hex = "#c8c011"; break;      // Xám
            default: hex = "#FFFFFF"; break;      // Trắng
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;
        return Color.white;
    }

}
