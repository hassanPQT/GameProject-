using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DG.Tweening;

public class SongWheelController : MonoBehaviour
{
    private static readonly int isSing = Animator.StringToHash("isSing");

    [Header("UI")]
    public RectTransform wheelRect;       // RectTransform của SongWheel
    public Image[] slices;                // Mảng 8 Image Slice0…Slice7

    [SerializeField] private GameManager gameManager; // Tham chiếu đến GameManager
    [SerializeField] private Animator animator;

    private List<int> selectSlices;
    private bool wheelActive = false;
    private int currentSlice = -1;
    private Vector2[] sliceSize;

    void Awake()
    {
        // Khởi tạo scale cho animation
        wheelRect.localScale = Vector3.zero;
    }

    void Start()
    {
        selectSlices = new List<int>();
        sliceSize = new Vector2[slices.Length];
        // Lưu kích thước ban đầu của từng slice
        for (int i = 0; i < slices.Length; i++)
        {
            sliceSize[i] = slices[i].GetComponent<RectTransform>().sizeDelta;
        }
    }

    void Update()
    {
        if (!wheelActive && Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ActivateWheel();
        }

        if (wheelActive)
        {
            UpdateSelection();

            if (Input.GetMouseButtonDown(0))
            {
                selectSlices.Add(currentSlice);
                if (gameManager != null && selectSlices.Count == 2)
                {
                    gameManager.OnPlayerSelect(selectSlices.ToArray());
                    selectSlices.Clear();
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

    public void ActivateWheel()
    {
        animator.SetBool(isSing, true);
        wheelActive = true;
        wheelRect.gameObject.SetActive(true);
        wheelRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // DOTween: scale pop-in
        wheelRect.localScale = Vector3.zero;
        wheelRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void ReleaseWheel()
    {
        animator.SetBool(isSing, false);
        wheelActive = false;

        // DOTween: scale pop-out then deactivate
        wheelRect.DOScale(0f, 0.15f).SetEase(Ease.InBack)
            .OnComplete(() => wheelRect.gameObject.SetActive(false));

        ResetHighlight();
        currentSlice = -1;
    }

    void UpdateSelection()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelRect, mousePos, null, out localPoint);

        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int slice = 0;
        if (angle >= 20 && angle < 70) slice = 7;
        if (angle >= 70 && angle < 110) slice = 0;
        if (angle >= 110 && angle < 160) slice = 1;
        if (angle >= 160 && angle < 200) slice = 2;
        if (angle >= 200 && angle < 250) slice = 3;
        if (angle >= 250 && angle < 290) slice = 4;
        if (angle >= 290 && angle < 340) slice = 5;
        if (angle >= 340 || angle < 20) slice = 6;

        if (slice != currentSlice)
        {
            HighlightSlice(slice);
            currentSlice = slice;
        }
    }

    void HighlightSlice(int slice)
    {
        for (int i = 0; i < slices.Length; i++)
            slices[i].GetComponent<RectTransform>().sizeDelta = sliceSize[i];

        slices[slice].GetComponent<RectTransform>().sizeDelta = new Vector2(
            sliceSize[slice].x + 10,
            sliceSize[slice].y + 10);
    }

    void ResetHighlight()
    {
        for (int i = 0; i < slices.Length; i++)
            slices[i].GetComponent<RectTransform>().sizeDelta = sliceSize[i];
    }
}
