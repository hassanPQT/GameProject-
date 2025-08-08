using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class SongWheelController : MonoBehaviour
{
    private static readonly int IsSing = Animator.StringToHash("isSing");

    [Header("UI")]
    [SerializeField] private RectTransform _wheelRect;
    [SerializeField] private Image[] _slices;

    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Animator _animator;

    private int _songWheelTime = 3000;
    private CancellationTokenSource _cts;
    private List<int> _selectSlices = new();
    private bool _wheelActive;
    private int _currentSlice = -1;
    private bool _mouseRightDelay = false;
    private Vector2[] _sliceSize;

    private void Awake()
    {
        _wheelRect.localScale = Vector3.zero;
        GAME_STAT.SONG_WHEEL_TIME = _songWheelTime;
    }

    private void Start()
    {
        _sliceSize = new Vector2[_slices.Length];
        for (int i = 0; i < _slices.Length; i++)
            _sliceSize[i] = _slices[i].GetComponent<RectTransform>().sizeDelta;
    }

    private void Update()
    {
        if (!_wheelActive && !_mouseRightDelay && Input.GetMouseButtonDown(1))
        {
            _mouseRightDelay = true;
            StartCoroutine(ResetLeftClickCooldown());

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ActivateWheel();
        }

        if (_wheelActive)
        {
            UpdateSelection();

            if (GameManager.Instance.Player.IsSignaling)
            {
                PlayerCountDownAnimation();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(_currentSlice);
                _selectSlices.Add(_currentSlice);
                if (_gameManager != null && _selectSlices.Count == 2)
                {
                    //OnPlayerResult(OnPlayerSelect(DirectionNumber));

                    _gameManager.OnPlayerSelect(_selectSlices.ToArray());
                    _selectSlices.Clear();
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                ReleaseWheel();
            }
        }
    }

    private void PlayerCountDownAnimation()
    {
        if (_currentSlice != -1 && _slices[_currentSlice].gameObject.activeSelf)
        {
            Image timerImage = _slices[_currentSlice].transform.GetChild(1).gameObject.GetComponent<Image>();
            float targetFill = GameManager.Instance.Timer;
            timerImage.DOKill();
            timerImage.DOFillAmount(targetFill, 3f)
                .OnComplete(() => timerImage.fillAmount = 0f);
        }
    }
    private IEnumerator ResetLeftClickCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        _mouseRightDelay = false;
    }
    public void ActivateWheel()
    {
        _animator.SetBool(IsSing, true);
        _wheelActive = true;
        _wheelRect.gameObject.SetActive(true);
        _wheelRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        _wheelRect.localScale = Vector3.zero;

        // Animation mở bánh xe
        _wheelRect.DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack);

        if (!GameManager.Instance.IsStop3s)// Cẩn thận chỗ này
            StartCoroutine(Delay3s());
    }
    public void ModifierSongWheelTime(float mult)
    {
        _songWheelTime = (int)(GAME_STAT.SONG_WHEEL_TIME * mult);
    }
    private IEnumerator Delay3s()
    {
        yield return new WaitForSeconds(_songWheelTime / 1000);
        if (_wheelActive)
            ReleaseWheel();
    }
    public void ReleaseWheel()
    {

        _animator.SetBool(IsSing, false);
        _wheelActive = false;
        _wheelRect.DOScale(0f, 0.15f).SetEase(Ease.InBack)
            .OnComplete(() => _wheelRect.gameObject.SetActive(false));

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

}
