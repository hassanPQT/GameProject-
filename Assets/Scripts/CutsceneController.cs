using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Cinemachine;
using Game.Scripts.Gameplay;

public class CutsceneController : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public MonoBehaviour playerController; // script điều khiển player (enable/disable)

    [Header("Cutscene Targets")]
    public Transform enemyTransform;        // optional, or pass in method
    public GameObject sword;                // sprite thanh kiếm trong scene
    public Transform playerHandPoint;       // vị trí trên player nơi sword đến (hand)

    [Header("Dialog UI")]
    public CanvasGroup dialogCanvasGroup;   // CanvasGroup for fade in/out
    public TMP_Text dialogText;                 // or use TMPro
    public TMP_Text dialogText2;               // optional second text

    [Header("Timings")]
    public float pickupRiseTime = 2f;
    public float pickupDropTime = 0.6f;
    public float holdSwordTime = 10f; // time to hold sword before dropping
    public float dialogShowTime = 2.5f;

    [Header("Options")]
    public bool pauseEnemiesDuringCutscene = true;
    public KeyCode skipKey = KeyCode.Space;

    [Header("StartCutscene2 (Bird)")]
    public Transform birdTransform;           // reference tới bird transform trong scene
    public GameObject birdSadEmotion;         // object (child) hiển thị sad emotion (ex: biểu tượng)
    public Vector3 birdLandOffset = new Vector3(0f, -1.2f, 0f); // offset so với vị trí hiện tại (hạ xuống)
    private float waitBeforeBird = 5f;         // thời gian chờ trước khi chim hạ xuống
    public float birdFlyDownTime = 0.8f;      // thời gian chim bay xuống
    public float birdWobbleDuration = 2f;     // thời gian wobble (lung lay)
    public float birdWobbleStrength = 8f;     // cường độ wobble (độ xoay)
    public Ease birdEase = Ease.InOutQuad;

    [Header("Dialog (Bird)")]
    private string birdDialogLine1 = "I wonder why that bird looks a bit sad";
    public string birdDialog = "...";
    private string birdDialogLine2 = "Why is this bird so quiet? Can I sing to cheer it up?";
    public float dialogFadeTime = 0.18f;
    public float dialogHoldTime = 3f;


    // internal state
    private Rigidbody2D _playerRb;
    private bool _wasPlayerControllerEnabled;
    private bool _isPlaying = false;
    private Vector3 _swordOriginalPos;
    private Quaternion _swordOriginalRot;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject not found. Make sure it has the 'Player' tag assigned.");
            }
        }

        if (player != null)
            _playerRb = player.GetComponent<Rigidbody2D>();

        if (sword != null)
        {
            _swordOriginalPos = sword.transform.position;
            _swordOriginalRot = sword.transform.rotation;
        }

        if (dialogCanvasGroup != null)
            dialogCanvasGroup.alpha = 0f;
    }

    //private void Update()
    //{
    //    if (_isPlaying && Input.GetKeyDown(skipKey))
    //    {
    //        // Skip cutscene; stop all tweens and restore
    //        StopAllCoroutines();
    //        DOTween.KillAll();
    //        EndCutsceneImmediate();
    //    }
    //}

    /// <summary>
    /// Public method to start cutscene. Pass enemy transform and the position2.
    /// </summary>
    public void StartCutscene()
    {
        if (_isPlaying) return;

        StartCoroutine(CutsceneRoutine());
    }

    public void StartCutscene2()
    {
        if (_isPlaying) return;

        StartCoroutine(CutsceneRoutine2());
    }

    private IEnumerator CutsceneRoutine2()
    {
        // 3) chờ trước khi bird bay xuống
        yield return new WaitForSeconds(waitBeforeBird);
        birdTransform.gameObject.GetComponent<CircleCollider2D>().enabled = true;
        player.GetComponent<PlayerController>().movement.StopPlayer();

        // 1) disable player input & freeze physics
        if (playerController != null) { _wasPlayerControllerEnabled = playerController.enabled; playerController.enabled = false; }
        if (_playerRb != null)
        {
            _playerRb.linearVelocity = Vector2.zero;
            // Wait until _grounded is true

            while (!player.GetComponent<PlayerController>().movement.GetCheckGround())
            {
                yield return null; // Wait for next frame
            }

            _playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        //if (pauseEnemiesDuringCutscene) PauseAllEnemies(true);



        // 4) bird flies down (simulate nặng nề bằng ease In + small delay)
        Vector3 targetPos = birdTransform.position + birdLandOffset;
        yield return birdTransform.DOMove(targetPos, birdFlyDownTime).SetEase(birdEase).WaitForCompletion();

        // 5) play wobble / lung lay: dùng rotate shake
        // Đổi sang local rotation nếu cần, ở đây ta sử dụng DOShakeRotation
        Tween wobbleTween = birdTransform.DORotate(new Vector3(0, 0, 0), 0.01f); // dummy để ensure type
        wobbleTween.Kill(); // kill asap - we will launch DOShakeRotation
        Tween shake = birdTransform.DOShakeRotation(birdWobbleDuration, new Vector3(0, 0, birdWobbleStrength), 10, 90, false);
        // Active sad emotion object (ví dụ: speech bubble hoặc sprite)
        if (birdSadEmotion != null) birdSadEmotion.SetActive(true);

        yield return shake.WaitForCompletion();

        dialogCanvasGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, 40);
        Vector2 temp = dialogCanvasGroup.GetComponent<RectTransform>().anchoredPosition; // save position
        // 6) Show dialog line 1
        if (dialogCanvasGroup != null && dialogText != null)
        {
            dialogText.text = birdDialogLine1;
            dialogCanvasGroup.DOKill();
            dialogCanvasGroup.DOFade(1f, dialogFadeTime);
            yield return new WaitForSeconds(dialogHoldTime);
            dialogCanvasGroup.DOFade(0f, dialogFadeTime);
            yield return new WaitForSeconds(dialogFadeTime);
        }
        else yield return new WaitForSeconds(dialogHoldTime);

        // Show dialog
        if (dialogCanvasGroup != null && dialogText != null)
        {
            if (birdSadEmotion != null) birdSadEmotion.SetActive(false); // hide sad emotion

            dialogCanvasGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(-130, 50);

            dialogText.text = birdDialog;
            dialogCanvasGroup.DOKill();
            dialogCanvasGroup.DOFade(1f, dialogFadeTime);
            yield return new WaitForSeconds(dialogHoldTime);
            dialogCanvasGroup.DOFade(0f, dialogFadeTime);
            yield return new WaitForSeconds(dialogFadeTime);
        }
        else yield return new WaitForSeconds(dialogHoldTime);

        // 7) Show dialog line 2
        if (dialogCanvasGroup != null && dialogText != null)
        {
            if (birdSadEmotion != null) birdSadEmotion.SetActive(true); // hide sad emotion

            dialogCanvasGroup.GetComponent<RectTransform>().anchoredPosition = temp; // restore position

            dialogText.text = birdDialogLine2;
            dialogCanvasGroup.DOKill();
            dialogCanvasGroup.DOFade(1f, dialogFadeTime);
            yield return new WaitForSeconds(dialogHoldTime);
            dialogCanvasGroup.DOFade(0f, dialogFadeTime);
            yield return new WaitForSeconds(dialogFadeTime);
        }
        else yield return new WaitForSeconds(dialogHoldTime);

        // 10) restore states
        if (birdSadEmotion != null) birdSadEmotion.SetActive(false); // hide sad emotion
        if (_playerRb != null) _playerRb.bodyType = RigidbodyType2D.Dynamic;
        if (playerController != null) playerController.enabled = _wasPlayerControllerEnabled;
        //if (pauseEnemiesDuringCutscene) PauseAllEnemies(false);

        player.GetComponent<PlayerController>().movement.UnStop();
        _isPlaying = false;
    }

    private IEnumerator CutsceneRoutine()
    {
        player.GetComponent<PlayerController>().movement.StopPlayer();
        _isPlaying = true;


        // 1. disable player input and freeze physics
        if (playerController != null)
        {
            _wasPlayerControllerEnabled = playerController.enabled;
            playerController.enabled = false;
        }
        if (_playerRb != null)
        {
            _playerRb.linearVelocity = Vector2.zero;
            _playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (sword != null && playerHandPoint != null)
        {
            yield return new WaitForSeconds(2f); // wait a bit before picking up

            Vector3 groundPos = _swordOriginalPos;

            Sequence s = DOTween.Sequence();
            s.Append(sword.transform.DOMove(playerHandPoint.position, pickupRiseTime).SetEase(Ease.OutCubic));
            s.AppendInterval(1f);
            yield return s.WaitForCompletion();

            // Attach sword to player's hand
            sword.transform.SetParent(playerHandPoint, true);
            sword.transform.localPosition = Vector3.zero;

            //player move to enemy with sword, when go to enemy position then the sword was flung away and player stop

            Animator anim = player.transform.GetChild(0).gameObject.GetComponent<Animator>();

            // Bật animation chạy
            anim.SetBool("isRun", true);

            // Player chạy tới enemy
            yield return player.transform.DOMove(new Vector2(106.5f, 0.004999876f), 3)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            // Dừng animation chạy
            anim.SetBool("isRun", false);

            //enemy di chuyển sang bên trái đẩy nhẹ thanh kiếm ra khỏi tay player
            Vector3 originalPos = enemyTransform.position;
            Vector3 leftPos = originalPos + Vector3.left * 1.5f;
            Sequence seq = DOTween.Sequence();
            seq.Append(enemyTransform.DOMove(leftPos, 0.25f).SetEase(Ease.Linear));
            seq.Append(enemyTransform.DOMove(originalPos, 0.25f).SetEase(Ease.Linear));

            // Kiếm rớt khỏi tay player
            sword.transform.SetParent(null);

            float duration = 2f;
            Vector3 start = player.transform.position;
            Vector3 end = start - new Vector3(13f, 0);

            // Song song: xoay kiếm liên tục
            Tween rotateTween = sword.transform.DORotate(
                new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360 // xoay 360 trong 2s (chậm hơn)
            ).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear); // xoay vô hạn

            yield return DOVirtual.Float(0f, 1f, duration, t =>
            {
                // Lerp vị trí ngang
                Vector3 pos = Vector3.Lerp(start, end, t);
                // Thêm parabol theo Y
                float height = 7f; // độ cao
                pos.y += height * (1 - (2 * t - 1) * (2 * t - 1)); // công thức parabola
                sword.transform.position = pos;
            }).WaitForCompletion();

            rotateTween.Kill(); // dừng xoay kiếm



            // Show dialog
            if (dialogCanvasGroup != null && dialogText != null)
            {
                dialogCanvasGroup.DOKill();
                // fade in
                dialogCanvasGroup.DOFade(1f, 0.5f);
                // wait either time or until player presses a key (after 3.5s)
                float t = 0f;
                bool dismissed = false;
                float minKeyDelay = 3.5f; // minimum time before allowing key press

                while (t < dialogShowTime && !dismissed)
                {
                    if (t >= minKeyDelay && Input.anyKeyDown) dismissed = true; // allow quick skip of dialog after delay
                    t += Time.deltaTime;
                    yield return null;
                }
                // fade out
                dialogCanvasGroup.DOFade(0f, 0.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(1); // wait a bit before next step
            GameManager.Instance.IsInputEnable = true;
            // Wait until you want to drop the sword (replace with your own condition)
            // Example: wait for dialog to finish
            float holdTime = 20f;
            float timer = 0f;
            bool reduced = false;
            dialogText2.gameObject.SetActive(true); // hide second text if not used
            while (timer < holdTime)
            {
                // Giữ chuột phải để giảm thời gian
                if (!reduced && Input.GetMouseButton(1))
                {
                    dialogText2.text = "Now hold \"Right Mouse\" and click \"Left Mouse\" to any direction you want:D";

                    if (holdTime - timer > 1f)
                    {
                        holdTime = timer + 10f;
                    }
                    reduced = true;
                }

                // Nếu đang giữ chuột phải và bấm chuột trái ở bất kỳ frame nào
                if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0))
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }


            // Drop the sword
            dialogText2.text = "You did it, yay! Now use your sing to redeem enemy and help anyone happy:D";
            sword.transform.SetParent(null, true);
            yield return new WaitForSeconds(4);
            dialogText2.text = "";
            dialogText2.gameObject.SetActive(false); // hide second text if not used
            player.GetComponent<PlayerController>().movement.UnStop();
        }

        if (_playerRb != null) _playerRb.bodyType = RigidbodyType2D.Dynamic;

        // 6. restore player control and physics
        if (playerController != null) playerController.enabled = _wasPlayerControllerEnabled;

        // 7. resume enemies
        enemyTransform.gameObject.GetComponent<CircleCollider2D>().enabled = true; // enable collider again
        enemyTransform.gameObject.GetComponent<EnemyController>().enabled = true; // enable collider again
        enemyTransform.gameObject.GetComponent<EnemySignal>().enabled = true; // enable collider again

        _isPlaying = false;
    }

    private void EndCutsceneImmediate()
    {
        // restore sword
        if (sword != null)
        {
            sword.transform.position = _swordOriginalPos;
            sword.transform.rotation = _swordOriginalRot;
        }

        // hide dialog
        if (dialogCanvasGroup != null)
            dialogCanvasGroup.alpha = 0f;

        // restore physics and input
        if (_playerRb != null) _playerRb.bodyType = RigidbodyType2D.Dynamic;
        if (playerController != null) playerController.enabled = _wasPlayerControllerEnabled;

        //if (pauseEnemiesDuringCutscene)
        //    PauseAllEnemies(false);

        _isPlaying = false;
    }
}
