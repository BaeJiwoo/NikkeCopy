using System;
using UnityEngine;

public sealed class RecruitmentView : BaseView
{
    [SerializeField] private RecruitmentCarousel carousel;

    public event Action<string, int> RecruitRequested;

    private void OnEnable()
    {
        if (carousel != null)
        {
            carousel.RecruitRequested += HandleRecruitRequested;
        }
    }

    private void OnDisable()
    {
        if (carousel != null)
        {
            carousel.RecruitRequested -= HandleRecruitRequested;
        }
    }

    private void HandleRecruitRequested(string bannerId, int count)
    {
        RecruitRequested?.Invoke(bannerId, count);
        Debug.Log($"[RECRUITMENT] Requested {count} pull(s) from {bannerId}.", this);
    }
}
