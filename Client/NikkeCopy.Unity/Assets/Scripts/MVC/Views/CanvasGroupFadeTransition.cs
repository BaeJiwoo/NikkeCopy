using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class CanvasGroupFadeTransition : ViewTransition
{
    [SerializeField, Min(0f)] private float enterDuration = 0.2f;
    [SerializeField, Min(0f)] private float exitDuration = 0.15f;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public override UniTask PlayEnterAsync()
    {
        return FadeAsync(0f, 1f, enterDuration);
    }

    public override UniTask PlayExitAsync()
    {
        return FadeAsync(1f, 0f, exitDuration);
    }

    private async UniTask FadeAsync(float from, float to, float duration)
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = from;
        if (duration <= 0f)
        {
            _canvasGroup.alpha = to;
            return;
        }

        var elapsed = 0f;
        var cancellationToken = this.GetCancellationTokenOnDestroy();
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            _canvasGroup.alpha = Mathf.LerpUnclamped(from, to, easing.Evaluate(progress));
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        _canvasGroup.alpha = to;
    }
}
