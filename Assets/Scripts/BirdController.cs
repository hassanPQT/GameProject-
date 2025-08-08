using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] private float moveDistance = 0.35f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float flySpeed = 10f;
    [SerializeField] private float stayDuration = 10f;
    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private GameObject happyMood; 

    [Header("Signal Effect")]
    [SerializeField] private GameObject[] signalEffectPrefab;
    [SerializeField] private float _effectDuration = 0.5f;
    [SerializeField] private float _effectMaxScale = 1.5f;
    [SerializeField] private bool _isMoving = true;
    public event Action<SongDirection[]> OnSignalDirection;
    public bool IsMoving => _isMoving;  
    private SongDirection[] _currentDir;
    private Vector3 _smoothVelocity = Vector3.zero;

    //void OnDisable()
    //{
    //    DOTween.KillAll();
    //}


    //public bool IsMovingInDirection => _ismo
    public void Start()
    {
        // restart signal direction when player false
        OnSignalDirection += GameManager.Instance.OnEnemySignal;
        _isMoving = false;
    }
    public void SetActiveMood() { 
        if (happyMood != null)
        {
            happyMood.SetActive(true);
            AutoDelay();
        }
    }

    private async void AutoDelay()
    {
            await System.Threading.Tasks.Task.Delay(7000);
            happyMood.SetActive(false);
    }

    public void SetInactiveMood() { 
        if (happyMood != null)
        {
            happyMood.SetActive(false);
        }
    }


	public bool SignalRandomDirection()
    {
        if (_isMoving) return true;
        Debug.Log("`  SignalRandomDirection` called in BirdController.");

        _currentDir = new SongDirection[2];
        for (int i = 0; i < _currentDir.Length; i++)
            _currentDir[i] = (SongDirection)GameManager.Instance.DirectionNumber[UnityEngine.Random.Range(0, GameManager.Instance.DirectionNumber.Length)];

        OnSignalDirection?.Invoke(_currentDir);
        StartCoroutine(MoveInDirection(_currentDir));
        return true;
    }

    private IEnumerator MoveInDirection(SongDirection[] dir)
    {
        _isMoving = true;
        yield return new WaitForSeconds(2f);
        foreach (var d in dir)
        {
            ShowSignalEffect(d);
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

    public void StopMovement()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider) collider.enabled = false;
        //enabled = false;
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
    private void Update()
    {
        FlyIntoPlayer();
    }
    public void FlyIntoPlayer()
    {
        Debug.Log("FlyIntoPlayer called in BirdController." + happyMood.activeSelf);
        if (!happyMood.activeSelf) return;
        Vector3 offset = Vector3.up * hoverHeight;
        var player = GameManager.Instance.Player;
        Vector3 targetPos = player.transform.position + offset;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

    }

    private void OnStayFinished()
    {
        Debug.Log("Bird stay time ended.");
        // Add next logic here
    }

    private void ShowSignalEffect(SongDirection dir)
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
