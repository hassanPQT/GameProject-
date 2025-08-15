using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class BirdController : AbstractEnemy
{
    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private AudioClip birdSfx;
    private int _helperCount;
    private Vector3 startPoint;

    protected override void Start()
    {
        base.Start();
        _helperCount = 0;
        startPoint = transform.position;
        _isMoving = false;
        IsWin = false;
    }


    private async void AutoDelay()
    {
        await System.Threading.Tasks.Task.Delay(6000);
        SetActiveMood(false);
        IsWin = false;
    }

    public void CloseCollider()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider) collider.isTrigger = true;
        //enabled = false;
    }

    public void MakeMovement()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider) collider.enabled = true;
        enabled = true;
    }


    private void Update()
    {
        DetectPlayer();
    }

    private IEnumerator CheckPlayerStay(PlayerController playerController)
    {
        Debug.Log("here is check stay");
        bool isStay = true;
        float timer = 0f;
        var temp = 3;
        while (timer < temp)
        {
            var dis = Vector2.Distance(playerController.transform.position, transform.position);
            if (dis > 5)
            {
                isStay = false;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (isStay)
        {
            yield return Play();
        }

    }

    public IEnumerator FlyIntoPlayer()
    {
        _helperCount--;
        Vector3 offset = Vector3.up * hoverHeight;
        var player = GameManager.Instance.Player;
        Debug.Log("can jump");
        player.movement.CanDoubleJump();

        float timer = 0f;
        while (timer < 10f)
        {
            timer += Time.deltaTime;
            //Debug.Log("timer: " + timer);
            Vector3 targetPos = player.transform.position + offset;
            if (!player.movement.CheckDoubleJump)
            {
                break;
            }
            transform.position = Vector3.Lerp(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (!player.movement.CheckDoubleJump)
        {
            //follow player for 2s  
            float timer2 = 0f;
            while (timer2 < 2f)
            {
                timer2 += Time.deltaTime;
                //Debug.Log("timer: " + timer);
                Vector3 targetPos = player.transform.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPos, _moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        player.movement.LockDoubleJump();
    }
    private IEnumerator ReturnToStartPoint()
    {
        transform.DOMove(transform.position, 2f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(2f);
        IsWin = false;
    }
    public override IEnumerator OnPlayerRequest(PlayerController playerController)
    {
        if (_helperCount > 0)
        {
            //Debug.Log(_helperCount + " Helper");
            playerController.detection.OnPlayerWin();
        }
        else
        {
            Debug.Log("check check 0");
            _helperCount = 2;
            yield return ReturnToStartPoint();
            yield return CheckPlayerStay(playerController);
        }

    }

    public override void OnWinning()
    {
        //_helperCount = 2;
        HelperPlayer();
    }

    private void HelperPlayer()
    {
        Debug.Log("Helper Player");
        IsWin = true;
        SetActiveMood(true);
        SetUnHappyMood(false);
        AutoDelay();
        CloseCollider();
        StartCoroutine(FlyIntoPlayer());
    }

    public override IEnumerator OnPlayerMissed()
    {
        _isMoving = false;
        yield return ShakeBird();
        AudioManager.Instance.PlaySFX(birdSfx);
    }

    private IEnumerator ShakeBird()
    {
        // Shake parameters
        float duration = 1f;
        float strength = 0.2f;
        int vibrato = 20;

        // Use DOTween's DOShakePosition for shaking effect
        transform.DOShakePosition(duration, strength, vibrato)
            .SetEase(Ease.Linear);

        // Wait for the shake to finish
        yield return new WaitForSeconds(duration);

        yield return Play();
    }

}
