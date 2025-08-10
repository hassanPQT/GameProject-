using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Cinemachine;
public class CutsceneController : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public MonoBehaviour playerController; // script điều khiển player (enable/disable)
    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera enemyCamera;

    [Header("Cutscene Targets")]
    public Transform enemyTransform;        // optional, or pass in method
    public Transform positionAfter;         // Position2: nơi player đứng sau cutscene
    public GameObject sword;                // sprite thanh kiếm trong scene
    public Transform playerHandPoint;       // vị trí trên player nơi sword đến (hand)

    [Header("Dialog UI")]
    public CanvasGroup dialogCanvasGroup;   // CanvasGroup for fade in/out
    public TMP_Text dialogText;                 // or use TMPro

    [Header("Timings")]
    public float pickupRiseTime = 2f;
    public float pickupDropTime = 0.6f;
    public float dialogShowTime = 2.5f;

    [Header("Options")]
    public bool pauseEnemiesDuringCutscene = true;
    public KeyCode skipKey = KeyCode.Space;

    // internal state
    private Rigidbody2D _playerRb;
    private bool _wasPlayerControllerEnabled;
    private bool _isPlaying = false;
    private Vector3 _swordOriginalPos;
    private Quaternion _swordOriginalRot;

    private void Awake()
    {
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

        // 1. Switch camera to enemy
        if (enemyCamera != null && enemyTransform != null)
        {
            enemyCamera.Priority = 20; // Higher than playerCamera
            Debug.Log($"Switching camera to enemy: {enemyCamera.Priority}");
            if (playerCamera != null) playerCamera.Priority = 10;
            yield return new WaitForSeconds(5f); // Focus on enemy for 5 seconds
        }

        // 2. Switch camera back to player
        if (playerCamera != null && player != null)
        {
            playerCamera.Priority = 20;
            if (enemyCamera != null) enemyCamera.Priority = 10;
        }

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

        // 3. Sword pick up attempt
        if (sword != null && playerHandPoint != null)
        {
            yield return new WaitForSeconds(2f); // wait a bit before picking up
            // raise sword slightly (simulate lifting), then drop
            // record ground drop position (you can set where sword should fall)
            Vector3 groundPos = _swordOriginalPos; // or sword.transform.position if moved
            // sequence: quick up to hand, then heavy drop to ground
            Sequence s = DOTween.Sequence();
            s.Append(sword.transform.DOMove(playerHandPoint.position, pickupRiseTime).SetEase(Ease.OutCubic));
            // small hold so player sees weight
            s.AppendInterval(1f);
            // drop: jump-like heavy fall to groundPos
            s.Append(sword.transform.DOMove(groundPos, pickupDropTime).SetEase(Ease.InBounce));
            yield return s.WaitForCompletion();
        }

        // 4. Show dialog
        if (dialogCanvasGroup != null && dialogText != null)
        {
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

        // 6. restore player control and physics
        if (_playerRb != null) _playerRb.isKinematic = false;
        if (playerController != null) playerController.enabled = _wasPlayerControllerEnabled;

        // 7. resume enemies
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
