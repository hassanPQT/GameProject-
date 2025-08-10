using DG.Tweening;
using Game.Scripts.Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SongWheelController : MonoBehaviour
{
    private static readonly int IsSing = Animator.StringToHash("isSing");

    [Header("UI")]
    [SerializeField] private RectTransform _wheelRect;
    [SerializeField] private Image[] _slices;
    [SerializeField] PlayerController _playerController;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _offsetY = 60f; // Khoảng cách dọc từ vị trí của người chơi đến bánh xe

    private int _songWheelTime = 3000;
    public int _songWheelTimeForDisplay = 3000;
    private CancellationTokenSource _cts;
    private List<int> _selectSlices = new();
    private bool _wheelActive;
    private int _currentSlice = -1;
    private Vector2[] _sliceSize;
    private int newSongWheelNumber;
    private float _currentFillAmount = 1f; // Add this field to your class

    private void Awake()
    {
        _wheelRect.localScale = Vector3.zero;
        GAME_STAT.SONG_WHEEL_TIME = _songWheelTime;
    }

    private void Start()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        newSongWheelNumber = 6;
        _sliceSize = new Vector2[_slices.Length];
        for (int i = 0; i < _slices.Length; i++)
            _sliceSize[i] = _slices[i].GetComponent<RectTransform>().sizeDelta;
    }
    private void Synchron()
    {
        var positionOnScreen = Camera.main.WorldToScreenPoint(_playerController.transform.position);
        _wheelRect.position = positionOnScreen + Vector3.up * _offsetY;
        //_playerController.transform.position
    }

    private void FixedUpdate()
    {
        Synchron();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            InputManager.Instance.UnlockCursor();
            OpenSongWheel();
        }

        if (GameManager.Instance._awaitingPlayerSelect && GameManager.Instance.Timer != _songWheelTime)
        {
            if (_songWheelTimeForDisplay <= 0)
                _songWheelTimeForDisplay = 4950;
            _songWheelTimeForDisplay -= (int)(Time.deltaTime * 1000f * 0.5f);
        }


        if (_wheelActive)
        {
            UpdateAnimation();
        }

        if (Input.GetMouseButtonDown(0))
        {
            OnSelectSongWheel();
        }

        if (Input.GetMouseButtonUp(1))
        {
            CloseSongWheel();
        }

    }

    private void UpdateAnimation()
    {
        UpdateSelection();
        if (GameManager.Instance._awaitingPlayerSelect && GameManager.Instance.Timer != _songWheelTime)
        {
            PlayerCountDownAnimation();
        }
    }

    private void OnSelectSongWheel()
    {
        if (!_wheelActive) return;
        if (_slices[_currentSlice].gameObject.activeSelf)
        {
            _selectSlices.Add(_currentSlice);
        }
        if (_gameManager != null && _selectSlices.Count == 2)
        {

            _gameManager.OnSongWheelSelect(_selectSlices.ToArray());
            _selectSlices.Clear();
        }
    }
    private void OpenSongWheel()
    {
        if (_wheelActive) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ActivateWheel();
    }
    private void CloseSongWheel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ReleaseWheel();
    }
    private void PlayerCountDownAnimation()
    {
        if (_currentSlice != -1 && _slices[_currentSlice].gameObject.activeSelf)
        {
            Image timerImage = _slices[_currentSlice].transform.GetChild(1).gameObject.GetComponent<Image>();
            float targetFill = Mathf.Clamp01((float)_songWheelTimeForDisplay / 4950f);
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, targetFill, Time.deltaTime * 8f); // 8f is smoothing speed
            timerImage.fillAmount = _currentFillAmount;
            Debug.Log($"Timer for fillAmount: {_songWheelTimeForDisplay} ms");
        }
    }


    public void ActivateNewSongWheel()
    {
        if (newSongWheelNumber < 1)
            return;
        _slices[newSongWheelNumber].gameObject.SetActive(true);
        newSongWheelNumber--;
    }
    public void ActivateWheel()
    {
        _animator.SetBool(IsSing, true);
        _wheelRect.gameObject.SetActive(true);
        _wheelRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        _wheelRect.localScale = Vector3.zero;

        // Animation mở bánh xe
        _wheelRect.DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack).OnComplete(() => { _wheelActive = true; });
    }
    public void ModifierSongWheelTime(float mult)
    {
        _songWheelTime = (int)(GAME_STAT.SONG_WHEEL_TIME * mult);
    }
    public void ReleaseWheel()
    {
        _animator.SetBool(IsSing, false);
        _wheelRect.DOScale(0f, 0.15f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _wheelRect.gameObject.SetActive(false);
                _wheelActive = false;
            });


        ResetHighlight();
        _currentSlice = -1;
    }

    private void UpdateSelection()
    {
        Vector2 mousePos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _wheelRect, mousePos, null, out Vector2 localPoint);

        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int slice = 0;
        if (angle >= 20 && angle < 70) slice = 7;
        else if (angle >= 70 && angle < 110) slice = 0;
        else if (angle >= 110 && angle < 160) slice = 1;
        else if (angle >= 160 && angle < 200) slice = 2;
        else if (angle >= 200 && angle < 250) slice = 3;
        else if (angle >= 250 && angle < 290) slice = 4;
        else if (angle >= 290 && angle < 340) slice = 5;
        else if (angle >= 340 || angle < 20) slice = 6;

        if (slice != _currentSlice)
        {
            HighlightSlice(slice);
            _currentSlice = slice;
        }
    }

    private void HighlightSlice(int slice)
    {
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
            {
                _slices[i].transform.GetChild(0).gameObject.SetActive(false);
                _slices[i].transform.GetChild(1).gameObject.SetActive(false);
                _slices[i].GetComponent<RectTransform>().sizeDelta = _sliceSize[i];
            }
        }

        if (!_slices[slice].gameObject.activeSelf)
        {
            return;
        }

        _slices[slice].GetComponent<RectTransform>().sizeDelta = _sliceSize[slice] + new Vector2(10, 10);

        _slices[slice].transform.GetChild(0).gameObject.SetActive(true);
        _slices[slice].transform.GetChild(1).gameObject.SetActive(true);
    }

    private void ResetHighlight()
    {
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
                _slices[i].GetComponent<RectTransform>().sizeDelta = _sliceSize[i];
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null; // Tránh sử dụng lại
    }

    public void OnPlayerWin()
    {
        _songWheelTimeForDisplay = 4950;
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
            {
                _slices[i].transform.GetChild(1).gameObject.GetComponent<Image>().fillAmount = 0;
            }
        }
    }
    public void OnPlayerLose()
    {
        _songWheelTimeForDisplay = 4950;
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
            {
                _slices[i].transform.GetChild(1).gameObject.GetComponent<Image>().fillAmount = 0;
            }
        }
    }
}
