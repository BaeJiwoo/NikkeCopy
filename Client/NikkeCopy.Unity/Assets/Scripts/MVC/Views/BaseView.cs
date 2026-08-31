using Cysharp.Threading.Tasks;
using UnityEngine;

public enum ViewReuseMode
{
    Reuse = 0,
    Recreate = 1
}

public enum ViewLifecycleState
{
    None = 0,
    Created = 1,
    Entered = 2,
    Exited = 3,
    Released = 4
}

public abstract class BaseView : MonoBehaviour
{
    [SerializeField] private ViewReuseMode reuseMode = ViewReuseMode.Reuse;
    [SerializeField] private ViewTransition transition;

    public ViewReuseMode ReuseMode => reuseMode;
    public ViewLifecycleState LifecycleState { get; private set; }

    internal async UniTask CreateAsync()
    {
        if (LifecycleState != ViewLifecycleState.None)
        {
            return;
        }

        BindViewModel();
        await OnCreatedAsync();
        LifecycleState = ViewLifecycleState.Created;
        ViewSystemLogger.Lifecycle(this, LifecycleState);
    }

    internal async UniTask EnterAsync()
    {
        if (LifecycleState == ViewLifecycleState.None)
        {
            await CreateAsync();
        }

        if (LifecycleState == ViewLifecycleState.Released || LifecycleState == ViewLifecycleState.Entered)
        {
            return;
        }

        await OnEnteringAsync();
        if (transition != null)
        {
            await transition.PlayEnterAsync();
        }

        LifecycleState = ViewLifecycleState.Entered;
        ViewSystemLogger.Lifecycle(this, LifecycleState);
    }

    internal async UniTask ExitAsync()
    {
        if (LifecycleState != ViewLifecycleState.Entered)
        {
            return;
        }

        if (transition != null)
        {
            await transition.PlayExitAsync();
        }

        await OnExitingAsync();
        LifecycleState = ViewLifecycleState.Exited;
        ViewSystemLogger.Lifecycle(this, LifecycleState);
    }

    internal async UniTask ReleaseAsync()
    {
        if (LifecycleState == ViewLifecycleState.Released || LifecycleState == ViewLifecycleState.None)
        {
            return;
        }

        if (LifecycleState == ViewLifecycleState.Entered)
        {
            await ExitAsync();
        }

        await OnReleasedAsync();
        LifecycleState = ViewLifecycleState.Released;
        ViewSystemLogger.Lifecycle(this, LifecycleState);
    }

    protected virtual void BindViewModel()
    {
    }

    protected virtual UniTask OnCreatedAsync()
    {
        return UniTask.CompletedTask;
    }

    protected virtual UniTask OnEnteringAsync()
    {
        return UniTask.CompletedTask;
    }

    protected virtual UniTask OnExitingAsync()
    {
        return UniTask.CompletedTask;
    }

    protected virtual UniTask OnReleasedAsync()
    {
        return UniTask.CompletedTask;
    }
}
