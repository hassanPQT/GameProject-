using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class BirdController : AbstractEnemy
{
    [SerializeField] private float hoverHeight = 1.5f;


    private int _canDoubleJumpCount = 2;
    public bool IsMoving => _isMoving;  
    private Vector3 startPoint;
    protected override void Start()
    {
        base.Start();
        _canDoubleJumpCount = 0;
        startPoint = transform.position;
        _isMoving = false;
        IsWin = false;
    }


    private async void AutoDelay()
    {
        await System.Threading.Tasks.Task.Delay(6000);
        SetActiveMood(false);
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
        bool isStay = true;
        float timer = 0f;
        var temp = 3;
        while (timer < temp)
        {
            var dis = Vector2.Distance(playerController.transform.position, transform.position);
            if (dis > 4)
            {
                isStay = false;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (isStay)
        {
           signal.SignalRandomDirection(_moveDistance, _moveDuration);
        }
        else
        {
            _isMoving = true;
        }
    }

    
    public IEnumerator FlyIntoPlayer()
    {
        Vector3 offset = Vector3.up * hoverHeight;
        var player = GameManager.Instance.Player;
        player.CanDoubleJump();
        
        float timer = 0f;
        while(timer < 5f)
        {
            timer += Time.deltaTime;
            Vector3 targetPos = player.transform.position + offset;
            if (!player.CheckDoubleJump)
                break;
            transform.position = Vector3.Lerp(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        player.LockDoubleJump();
        StartCoroutine(ReturnToStartPoint());
    }
    private IEnumerator ReturnToStartPoint()
    {
        transform.DOMove(startPoint, 2f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(2f);
        IsWin = false;
    }
    public override void OnPlayerRequest(PlayerController playerController)
    {
        if (_canDoubleJumpCount > 0)
        {
            _canDoubleJumpCount--;
            StartCoroutine(FlyIntoPlayer());
        } else
        {
            _isMoving = false;
            StartCoroutine(CheckPlayerStay(playerController));
        }
    }

    public override void OnWinning()
    {
        IsWin = true;
        SetActiveMood(true);
        AutoDelay();
        CloseCollider();
        _canDoubleJumpCount = 1;
        StartCoroutine(FlyIntoPlayer());
    }

    public override void OnPlayerMissed()
    {
        _isMoving = false;
    }

}
