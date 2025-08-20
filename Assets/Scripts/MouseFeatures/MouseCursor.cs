// MouseCursor.cs
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MouseCursor : IDisposable
{
    private RectTransform _selector;
    private RectTransform _activeWheel;
    private float _selectorMargin = 8f;
    private bool _smooth = true;
    private float _smoothTime = 0.06f;
    private bool _snapToSlice = false;
    private float _selectorRadius = 50f;
    private bool _visible = false;

    // optional slice center angles (length must match slices if snap used)
    private float[] _sliceCenterAngles;

    public MouseCursor(RectTransform selector, float selectorMargin = 8f, bool smooth = true, float smoothTime = 0.06f, bool snapToSlice = false)
    {
        _selector = selector;
        _selectorMargin = selectorMargin;
        _smooth = smooth;
        _smoothTime = smoothTime;
        _snapToSlice = snapToSlice;

        if (_selector != null)
        {
            _selector.gameObject.SetActive(false);
            // ensure pivot center for anchor pos movement
            _selector.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    public void SetSliceCenterAngles(float[] angles)
    {
        _sliceCenterAngles = angles;
    }

    public void SetActiveWheel(RectTransform parentWheel)
    {
        if (_selector == null || parentWheel == null) return;

        // reparent while keeping local layout
        if (_selector.parent != parentWheel)
            _selector.SetParent(parentWheel, false);

        _activeWheel = parentWheel;
        ComputeRadius(parentWheel);

        Show();
    }

    private void ComputeRadius(RectTransform wheel)
    {
        if (wheel == null || _selector == null) return;

        // choose the smaller half extent (width or height) and account for lossyScale
        float wheelHalf = Mathf.Min(wheel.rect.width * 0.5f * wheel.lossyScale.x, wheel.rect.height * 0.5f * wheel.lossyScale.y);
        float selectorHalf = (_selector != null) ? (_selector.rect.width * 0.5f) * _selector.lossyScale.x : 0f;
        _selectorRadius = Mathf.Max(10f, wheelHalf - selectorHalf - _selectorMargin);
    }

    public void UpdateSelector(float angleDeg, int sliceIndex)
    {
        if (_selector == null || _activeWheel == null) return;

        Vector2 targetLocal;

        if (_snapToSlice && _sliceCenterAngles != null && sliceIndex >= 0 && sliceIndex < _sliceCenterAngles.Length)
        {
            float sliceAngle = _sliceCenterAngles[sliceIndex];
            float rad = sliceAngle * Mathf.Deg2Rad;
            Vector2 dirSnap = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            targetLocal = dirSnap * _selectorRadius;
        }
        else
        {
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            targetLocal = dir * _selectorRadius;
        }

        if (!_visible) Show();

        // ensure we kill previous tweens on this rect
        _selector.DOKill();

        if (_smooth)
            _selector.DOAnchorPos(targetLocal, _smoothTime).SetEase(Ease.OutQuad);
        else
            _selector.anchoredPosition = targetLocal;
    }

    public void Show()
    {
        if (_selector == null) return;
        _selector.gameObject.SetActive(true);
        _visible = true;
    }

    public void Hide(bool killTween = true)
    {
        if (_selector == null) return;
        if (killTween) _selector.DOKill();
        _selector.gameObject.SetActive(false);
        _visible = false;
    }

    public void SetSnapToSlice(bool snap) => _snapToSlice = snap;
    public void SetSmooth(bool smooth) => _smooth = smooth;
    public void SetSmoothTime(float t) => _smoothTime = t;
    public void SetMargin(float m) { _selectorMargin = m; if (_activeWheel != null) ComputeRadius(_activeWheel); }

    public void Dispose()
    {
        if (_selector != null) _selector.DOKill();
        _selector = null;
        _activeWheel = null;
        _sliceCenterAngles = null;
    }
}
