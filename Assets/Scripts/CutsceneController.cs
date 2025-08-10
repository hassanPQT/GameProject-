using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#if CINEMACHINE
using Cinemachine;
using UnityEngine.Playables;
#endif

public class CutsceneController : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public MonoBehaviour playerController; // script điều khiển player (enable/disable)
    public Camera mainCamera;
    public bool useCinemachine = false;
#if CINEMACHINE
    public CinemachineVirtualCamera vCam;
    private Transform _prevVCamFollow;
#endif

    [Header("Cutscene Targets")]
    public Transform enemyTransform;        // optional, or pass in method
    public Transform positionAfter;         // Position2: nơi player đứng sau cutscene
    public GameObject sword;                // sprite thanh kiếm trong scene
    public Transform playerHandPoint;       // vị trí trên player nơi sword đến (hand)

    [Header("Dialog UI")]
    public CanvasGroup dialogCanvasGroup;   // CanvasGroup for fade in/out
    public Text dialogText;                 // or use TMPro

    [Header("Timings")]
    public float camMoveTime = 0.7f;
    public float lookAtDuration = 1.2f;
    public float pickupRiseTime = 0.25f;
    public float pickupDropTime = 0.6f;
    public float dialogShowTime = 2.5f;

    [Header("Options")]
    public bool pauseEnemiesDuringCutscene = true;
    public KeyCode skipKey = KeyCode.Space;

    // internal state
    private Rigidbody2D _playerRb;
    private bool _wasPlayerControllerEnabled;
    private bool _isPlaying = false;
    private Vector3 _mainCamOriginalPos;
    private float _mainCamZ;
    private Vector3 _swordOriginalPos;
    private Quaternion _swordOriginalRot;

    private void Awake()
    {
        if (player != null)
            _playerRb = player.GetComponent<Rigidbody2D>();

        if (mainCamera != null)
            _mainCamZ = mainCamera.transform.position.z;

        if (sword != null)
        {
            _swordOriginalPos = sword.transform.position;
            _swordOriginalRot = sword.transform.rotation;
        }

        if (dialogCanvasGroup != null)
            dialogCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (_isPlaying && Input.GetKeyDown(skipKey))
        {
            // Skip cutscene; stop all tweens and restore
            StopAllCoroutines();
            DOTween.KillAll();
            EndCutsceneImmediate();
        }
    }

    /// <summary>
    /// Public method to start cutscene. Pass enemy transform and the position2.
    /// </summary>
    public void StartCutscene(Transform enemy, Transform posAfter)
    {
        if (_isPlaying) return;

        enemyTransform = enemy;
        positionAfter = posAfter;
        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
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
            _playerRb.isKinematic = true;
        }

        // 2. pause enemies if needed
        if (pauseEnemiesDuringCutscene)
            PauseAllEnemies(true);

        // store camera original pos
        _mainCamOriginalPos = mainCamera.transform.position;

#if CINEMACHINE
        if (useCinemachine && vCam != null)
        {
            // store previous follow and set to enemy to focus
            _prevVCamFollow = vCam.Follow;
            vCam.Follow = enemyTransform;
            // wait a bit for vcam to blend in
            yield return new WaitForSeconds(camMoveTime);
        }
        else
#endif
        {
            // move main camera to enemy
            Vector3 enemyCamPos = new Vector3(enemyTransform.position.x, enemyTransform.position.y, _mainCamZ);
            yield return mainCamera.transform.DOMove(enemyCamPos, camMoveTime).SetEase(Ease.OutQuad).WaitForCompletion();
        }

        // small look time so player sees enemy
        yield return new WaitForSeconds(lookAtDuration);

#if CINEMACHINE
        if (useCinemachine && vCam != null)
        {
            // restore follow to player
            vCam.Follow = player.transform;
            yield return new WaitForSeconds(camMoveTime);
        }
        else
#endif
        {
            // move camera back to player
            Vector3 playerCamPos = new Vector3(player.transform.position.x, player.transform.position.y, _mainCamZ);
            yield return mainCamera.transform.DOMove(playerCamPos, camMoveTime).SetEase(Ease.InQuad).WaitForCompletion();
        }

        // 3. Sword pick up attempt
        if (sword != null && playerHandPoint != null)
        {
            // raise sword slightly (simulate lifting), then drop
            // record ground drop position (you can set where sword should fall)
            Vector3 groundPos = _swordOriginalPos; // or sword.transform.position if moved
            // sequence: quick up to hand, then heavy drop to ground
            Sequence s = DOTween.Sequence();
            s.Append(sword.transform.DOMove(playerHandPoint.position, pickupRiseTime).SetEase(Ease.OutCubic));
            // small hold so player sees weight
            s.AppendInterval(0.25f);
            // drop: jump-like heavy fall to groundPos
            s.Append(sword.transform.DOMove(groundPos, pickupDropTime).SetEase(Ease.InBounce));
            yield return s.WaitForCompletion();
        }

        // 4. Show dialog
        if (dialogCanvasGroup != null && dialogText != null)
        {
            dialogText.text = "Tôi chỉ biết hát thôi… giờ làm sao 😨";
            dialogCanvasGroup.DOKill();
            // fade in
            dialogCanvasGroup.DOFade(1f, 0.2f);
            // wait either time or until player presses a key
            float t = 0f;
            bool dismissed = false;
            while (t < dialogShowTime && !dismissed)
            {
                if (Input.anyKeyDown) dismissed = true; // allow quick skip of dialog
                t += Time.deltaTime;
                yield return null;
            }
            // fade out
            dialogCanvasGroup.DOFade(0f, 0.2f);
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        // 5. Teleport player to positionAfter (Position2)
        if (positionAfter != null)
            player.transform.position = positionAfter.position;

        // 6. restore camera to player (if not already)
#if CINEMACHINE
        if (useCinemachine && vCam != null)
        {
            vCam.Follow = player.transform;
            yield return new WaitForSeconds(0.1f);
        }
        else
#endif
        {
            Vector3 restored = new Vector3(player.transform.position.x, player.transform.position.y, _mainCamZ);
            yield return mainCamera.transform.DOMove(restored, camMoveTime / 1.5f).SetEase(Ease.OutQuad).WaitForCompletion();
        }

        // 7. restore player control and physics
        if (_playerRb != null) _playerRb.isKinematic = false;
        if (playerController != null) playerController.enabled = _wasPlayerControllerEnabled;

        // 8. resume enemies
        if (pauseEnemiesDuringCutscene)
            PauseAllEnemies(false);

        _isPlaying = false;
    }

    private void PauseAllEnemies(bool pause)
    {
        // very simple implementation: find all EnemyController and enable/disable
        var enemies = FindObjectsOfType<MonoBehaviour>(); // filter for your enemy class
        foreach (var e in enemies)
        {
            // replace "EnemyController" with your actual enemy script type
            if (e.GetType().Name == "EnemyController")
            {
                e.enabled = !pause;
            }
        }
    }

    private void EndCutsceneImmediate()
    {
        // restore camera
#if CINEMACHINE
        if (useCinemachine && vCam != null)
        {
            vCam.Follow = player.transform;
        }
        else
#endif
        {
            mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, _mainCamZ);
        }

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
        if (_playerRb != null) _playerRb.isKinematic = false;
        if (playerController != null) playerController.enabled = _wasPlayerControllerEnabled;

        if (pauseEnemiesDuringCutscene)
            PauseAllEnemies(false);

        // teleport player to positionAfter as fallback
        if (positionAfter != null)
            player.transform.position = positionAfter.position;

        _isPlaying = false;
    }
}
