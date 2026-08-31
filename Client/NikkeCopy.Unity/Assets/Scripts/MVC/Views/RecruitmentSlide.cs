using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecruitmentSlide : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private Button singleRecruitButton;
    [SerializeField] private Button tenRecruitButton;
    [SerializeField] private string bannerId;
    [SerializeField] private string bannerTitle;
    [SerializeField, TextArea] private string bannerDescription;
    [SerializeField] private Color bannerColor = Color.gray;

    public event Action<string, int> RecruitRequested;

    private void Awake()
    {
        singleRecruitButton.onClick.AddListener(() => RecruitRequested?.Invoke(bannerId, 1));
        tenRecruitButton.onClick.AddListener(() => RecruitRequested?.Invoke(bannerId, 10));
        ApplyPresentation();
    }

    public void Configure(string bannerId, string title, string description, Color color)
    {
        this.bannerId = bannerId;
        bannerTitle = title;
        bannerDescription = description;
        bannerColor = color;

        if (Application.isPlaying)
        {
            ApplyPresentation();
        }
    }

    private void ApplyPresentation()
    {
        titleLabel.text = bannerTitle;
        descriptionLabel.text = bannerDescription;
        background.color = bannerColor;
    }
}
