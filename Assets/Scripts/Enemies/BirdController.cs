using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class BirdController : AbstractEnemy
{
    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private AudioClip birdSfx;
    private int _helperCount = 2;
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
            if (dis >  10)
            {
                isStay = false;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (isStay)
        {
            Play();
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
        while(timer < 5f)
        {
            timer += Time.deltaTime;
            Vector3 targetPos = player.transform.position + offset;
            if (!player.movement.CheckDoubleJump)
                break;
            transform.position = Vector3.Lerp(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        player.movement.LockDoubleJump();
    }
    private IEnumerator ReturnToStartPoint()
    {
        transform.DOMove(startPoint, 2f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(2f);
        IsWin = false;
    }
    public override void OnPlayerRequest(PlayerController playerController)
    {
        if (_helperCount > 0)
        {
            Debug.Log(_helperCount + " Helper");
            _helperCount--;
            playerController.detection.OnPlayerWin();
        }
        else {
            Debug.Log("check check 0");
            _helperCount = 2;
            StartCoroutine(ReturnToStartPoint());
            StartCoroutine(CheckPlayerStay(playerController));    
        }

    }

    public override void OnWinning()
    {
        //_helperCount = 2;
        HelperPlayer();
    }

    private void HelperPlayer()
    {
        IsWin = true;
        SetActiveMood(true);
        SetUnHappyMood(false);
        AutoDelay();
        CloseCollider();
        StartCoroutine(FlyIntoPlayer());
    }

    public override void OnPlayerMissed()
    {
        _isMoving = false;

        // Shake the bird for ~1 second
        StartCoroutine(ShakeBird());
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

        Play();
    }
}
