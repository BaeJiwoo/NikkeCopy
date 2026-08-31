using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class RecruitmentCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private List<RecruitmentSlide> slides = new();
    [SerializeField, Range(0.05f, 0.5f)] private float snapThreshold = 0.18f;
    [SerializeField, Min(0.05f)] private float snapDuration = 0.25f;

    private readonly List<RectTransform> _slideRects = new();
    private Coroutine _snapRoutine;
    private Vector2 _dragStartPointer;
    private float _dragStartContentX;
    private int _selectedIndex;
    private float _lastViewportWidth;

    public event Action<string, int> RecruitRequested;

    private void Awake()
    {
        _slideRects.Clear();
        foreach (var slide in slides)
        {
            if (slide == null) continue;
            slide.RecruitRequested += HandleRecruitRequested;
            _slideRects.Add((RectTransform)slide.transform);
        }
    }

    private void OnDestroy()
    {
        foreach (var slide in slides)
        {
            if (slide != null) slide.RecruitRequested -= HandleRecruitRequested;
        }
    }

    private void HandleRecruitRequested(string bannerId, int count) => RecruitRequested?.Invoke(bannerId, count);

    private void Start()
    {
        RefreshLayout(true);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled && viewport != null && !Mathf.Approximately(_lastViewportWidth, viewport.rect.width))
        {
            RefreshLayout(true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopSnap();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out _dragStartPointer);
        _dragStartContentX = content.anchoredPosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out var pointer);
        var delta = pointer.x - _dragStartPointer.x;
        var minX = -Mathf.Max(0, slides.Count - 1) * viewport.rect.width;
        content.anchoredPosition = new Vector2(Mathf.Clamp(_dragStartContentX + delta, minX, 0f), 0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out var pointer);
        var delta = pointer.x - _dragStartPointer.x;
        var requiredDelta = viewport.rect.width * snapThreshold;

        if (Mathf.Abs(delta) >= requiredDelta)
        {
            _selectedIndex += delta < 0f ? 1 : -1;
        }

        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, slides.Count - 1));
        SnapTo(_selectedIndex);
    }

    private void RefreshLayout(bool keepSelectedSlide)
    {
        if (viewport == null || content == null)
        {
            return;
        }

        var width = viewport.rect.width;
        var height = viewport.rect.height;
        if (width <= 0f || height <= 0f)
        {
            return;
        }

        _lastViewportWidth = width;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * _slideRects.Count);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        for (var index = 0; index < _slideRects.Count; index++)
        {
            var slide = _slideRects[index];
            slide.anchorMin = new Vector2(0f, 0f);
            slide.anchorMax = new Vector2(0f, 1f);
            slide.pivot = new Vector2(0f, 0.5f);
            slide.sizeDelta = new Vector2(width, 0f);
            slide.anchoredPosition = new Vector2(width * index, 0f);
        }

        if (keepSelectedSlide)
        {
            content.anchoredPosition = new Vector2(-_selectedIndex * width, 0f);
        }
    }

    private void SnapTo(int index)
    {
        StopSnap();
        _snapRoutine = StartCoroutine(SnapRoutine(-index * viewport.rect.width));
    }

    private IEnumerator SnapRoutine(float targetX)
    {
        var startX = content.anchoredPosition.x;
        var elapsed = 0f;
        while (elapsed < snapDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / snapDuration));
            content.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, progress), 0f);
            yield return null;
        }

        content.anchoredPosition = new Vector2(targetX, 0f);
        _snapRoutine = null;
    }

    private void StopSnap()
    {
        if (_snapRoutine != null)
        {
            StopCoroutine(_snapRoutine);
            _snapRoutine = null;
        }
    }
}
