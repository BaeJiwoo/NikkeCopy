using UnityEngine;

internal static class ViewSystemLogger
{
    private const string Color = "#70B7FF";

    public static bool Enabled { get; set; } = true;

    public static void Lifecycle(BaseView view, ViewLifecycleState state)
    {
        Info($"{view.GetType().Name} → {state}", view);
    }

    public static void Info(string message, Object context = null)
    {
        if (!Enabled)
        {
            return;
        }

        Debug.Log($"<color={Color}><b>[VIEW]</b></color> {message}", context);
    }
}
