using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Cinemachine;
using UnityEditor.Rendering;
using Unity.Play.Publisher.Editor;
using Game.Scripts.Gameplay;
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
    public TMP_Text dialogText2;               // optional second text

    [Header("Timings")]
    public float pickupRiseTime = 2f;
    public float pickupDropTime = 0.6f;
    public float holdSwordTime = 10f; // time to hold sword before dropping
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
        StartCoroutine(player.GetComponent<PlayerController>().PausePlayer(5));
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

        if (sword != null && playerHandPoint != null)
        {
            yield return new WaitForSeconds(2f); // wait a bit before picking up

            Vector3 groundPos = _swordOriginalPos;

            Sequence s = DOTween.Sequence();
            s.Append(sword.transform.DOMove(playerHandPoint.position, pickupRiseTime).SetEase(Ease.OutCubic));
            s.AppendInterval(1f);
            yield return s.WaitForCompletion();

            // Attach sword to player's hand
            dialogText2.gameObject.SetActive(true); // hide second text if not used
            sword.transform.SetParent(playerHandPoint, true);
            sword.transform.localPosition = Vector3.zero;

            // Wait until you want to drop the sword (replace with your own condition)
            // Example: wait for dialog to finish
            float holdTime = 20f;
            float timer = 0f;
            bool reduced = false;

            while (timer < holdTime)
            {
                // Giữ chuột phải để giảm thời gian
                if (!reduced && Input.GetMouseButton(1))
                {
                    dialogText2.text = "Now hold right move and click left mouse to any direction you want:D";

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
            dialogText2.gameObject.SetActive(false); // hide second text if not used
            sword.transform.SetParent(null, true);
            sword.transform.DOMove(groundPos, pickupDropTime).SetEase(Ease.InBounce);
            yield return new WaitForSeconds(pickupDropTime);
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

        //// 7. resume enemies
        //if (pauseEnemiesDuringCutscene)
        //    PauseAllEnemies(false);

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
