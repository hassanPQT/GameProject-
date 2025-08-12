using Game.Scripts.Gameplay;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetection : MonoBehaviour
{
    [SerializeField] Image _awaitImage;
    private PlayerController playerController;
    private AbstractEnemy currentEnemy;
    private SongDirection[] _targetDir;

    public float TimeOut;
    private float _inputTimeout = 10f;
    private bool _awaitingInput;

    private bool _selected;

    private void Awake()
    {
        _awaitImage.gameObject.SetActive(false);
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        DetectEnemy();
    }

    private void DetectEnemy()
    {
        if (currentEnemy != null)
        {
            if (Vector2.Distance(transform.position, currentEnemy._startPoint) > 10)
            {
                currentEnemy = null;
            }
            return;
        }
            Collider2D[] hits = GetEnemies();

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<AbstractEnemy>();
            if (enemy != null)
            {
                if (enemy.IsWin) continue;
                else
                {
                    OnDetectEnemy(enemy);
                }
            }
        }

    }
    public bool IsPlaying()
    {
        return currentEnemy != null;
    }
    private void OnDetectEnemy(AbstractEnemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.Singal += x => OnEnemySignal(x);
        StartPlay();
    }

    private void OnEnemySignal(SongDirection[] songDirection)
    {
        Debug.Log("on signal");
        _targetDir = songDirection;
        Debug.Log("asd" + currentEnemy.Singal.Method.Name);
        StartCoroutine(AwaitInput(10));
    }
    private IEnumerator AwaitInput(float timeOut)
    {
        _awaitImage.gameObject.SetActive(true);
        _awaitImage.color = Color.white;
        float t = 0;  
        while (t < timeOut)
        {
            if (_selected) break;
            _awaitImage.fillAmount = 1 - t / timeOut;
            if (t/timeOut > 0.5f)
            {
                _awaitImage.color = Color.red;
            }
            t += Time.deltaTime;
            yield return null;
        }

        _awaitImage.gameObject.SetActive(false);
        if (!_selected)
        {
            OnPayerLose();
        }
    }
    private Collider2D[] GetEnemies()
    {
        float detectRadius = 3.5f;
        LayerMask enemyLayer = LayerMask.GetMask("Flying");
        DrawDebugCircle(transform.position, detectRadius, Color.red);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
        return hits;
    }

    public void SelectSongWheel(int[] sliceIndex)
    {
        _selected = true;
        bool result = CheckCondition(sliceIndex);
        OnPlayerResult(result);
    }

    private void OnPlayerResult(bool result)
    {
        if (result)
        {
            OnPlayerWin();
        }
        else
        {
            OnPayerLose();
        }
    }

    private bool CheckCondition(int[] sliceIndex)
    {
        bool correct = sliceIndex.Length == 2 && _targetDir != null && sliceIndex.Length == _targetDir.Length;
        if (correct)
        {
            for (int i = 0; i < sliceIndex.Length; i++)
            {
                if (sliceIndex[i] != (int)_targetDir[i])
                {
                    correct = false;
                    break;
                }
            }
        }
        return correct;
    }

    private void StartPlay()
    {
        Debug.Log("start play");
        currentEnemy?.OnPlayerRequest(playerController);
    }
    public void OnPlayerWin()
    {
        playerController.movement.UnStop();
        currentEnemy.OnWinning();
        currentEnemy = null;
        //_selected = false;
    }
    public void OnPayerLose()
    {
        Debug.Log("lose play");

        if (currentEnemy == null) return;
        currentEnemy.OnPlayerMissed();
    }
    private void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        for (int i = 1; i <= segments; i++)
        {
            angle += 2 * Mathf.PI / segments;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Debug.DrawLine(lastPoint, nextPoint, color, 0.01f);
            lastPoint = nextPoint;
        }
    }
}