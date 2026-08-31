using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class ViewManager : MonoBehaviour
{
    private static ViewManager _instance;
    public static ViewManager Instance => _instance;

    [Header("View Setup")]
    [SerializeField] private Transform viewRoot;
    [SerializeField] private BaseView initialView;
    [SerializeField] private List<BaseView> viewPrefabs = new();

    [Header("Transition Input Guard")]
    [SerializeField] private GameObject raycastBlocker;

    [Header("Diagnostics")]
    [SerializeField] private bool enableLogging = true;

    private readonly Dictionary<Type, BaseView> _viewCache = new();
    private BaseView _currentView;
    private BaseView _previousView;
    private bool _isTransitioning;

    public BaseView CurrentView => _currentView;
    public BaseView PreviousView => _previousView;
    public IReadOnlyList<BaseView> ViewPrefabs => viewPrefabs;
    public bool CanGoBack => _previousView != null;
    public bool IsTransitioning => _isTransitioning;
    public event Action<BaseView> CurrentViewChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("Only one ViewManager can exist at a time.", this);
            Destroy(gameObject);
            return;
        }

        _instance = this;
        ViewSystemLogger.Enabled = enableLogging;
        if (viewRoot == null)
        {
            viewRoot = transform;
        }

        SetInputBlocked(false);
        ValidateViewPrefabs();
        RegisterInitialView();
    }

    private void Start()
    {
        if (initialView != null)
        {
            EnterInitialViewAsync().Forget();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public UniTask<bool> PushAsync<T>(params Func<UniTask>[] beforeTransition) where T : BaseView
    {
        return PushAsync(typeof(T), beforeTransition);
    }

    public UniTask<bool> PushAsync(BaseView targetView, params Func<UniTask>[] beforeTransition)
    {
        if (targetView == null)
        {
            Debug.LogError("Target View is not assigned.", this);
            return UniTask.FromResult(false);
        }

        return PushAsync(targetView.GetType(), beforeTransition);
    }

    private async UniTask<bool> PushAsync(Type viewType, Func<UniTask>[] beforeTransition)
    {
        if (_isTransitioning)
        {
            return false;
        }

        if (_currentView != null && _currentView.GetType() == viewType)
        {
            ViewSystemLogger.Info($"Push ignored because {viewType.Name} is already current.", this);
            return false;
        }

        _isTransitioning = true;
        SetInputBlocked(true);
        var currentView = CurrentView;
        ViewSystemLogger.Info($"Push requested: {currentView?.GetType().Name ?? "None"} → {viewType.Name}", this);

        try
        {
            if (beforeTransition != null && beforeTransition.Length > 0)
            {
                ViewSystemLogger.Info($"Running {beforeTransition.Length} task(s) before opening {viewType.Name}.", this);
            }

            await RunBeforeTransitionTasksAsync(beforeTransition);
            if (beforeTransition != null && beforeTransition.Length > 0)
            {
                ViewSystemLogger.Info($"Pre-transition tasks completed: {viewType.Name}", this);
            }

            var nextView = await LoadViewAsync(viewType);

            if (currentView != null)
            {
                await currentView.ExitAsync();
                currentView.gameObject.SetActive(false);
            }

            nextView.gameObject.SetActive(true);

            try
            {
                await nextView.EnterAsync();
                var discardedView = _previousView;
                _previousView = currentView;
                _currentView = nextView;
                await ReleaseDiscardedViewAsync(discardedView, _currentView, _previousView);
                CurrentViewChanged?.Invoke(_currentView);
                ViewSystemLogger.Info(
                    $"Push completed: {_currentView.GetType().Name} (previous: {_previousView?.GetType().Name ?? "None"})",
                    this);
                return true;
            }
            catch
            {
                nextView.gameObject.SetActive(false);
                if (nextView.ReuseMode == ViewReuseMode.Recreate)
                {
                    await nextView.ReleaseAsync();
                    Destroy(nextView.gameObject);
                }

                await RestoreViewAsync(currentView);
                throw;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"[VIEW] Push failed. Current View remains {CurrentView?.GetType().Name ?? "None"}.", this);
            Debug.LogException(exception, this);
            return false;
        }
        finally
        {
            SetInputBlocked(false);
            _isTransitioning = false;
        }
    }

    public async UniTask<bool> PopAsync()
    {
        if (_isTransitioning || _previousView == null)
        {
            return false;
        }

        _isTransitioning = true;
        SetInputBlocked(true);
        var currentView = _currentView;
        var previousView = _previousView;
        ViewSystemLogger.Info($"Pop requested: {currentView.GetType().Name}", this);

        try
        {
            await currentView.ExitAsync();
            currentView.gameObject.SetActive(false);
            previousView.gameObject.SetActive(true);

            try
            {
                await previousView.EnterAsync();

                _currentView = previousView;
                _previousView = null;
                CurrentViewChanged?.Invoke(_currentView);

                await ReleaseDiscardedViewAsync(currentView, _currentView, _previousView);

                ViewSystemLogger.Info($"Pop completed: {_currentView.GetType().Name} (previous: None)", this);
                return true;
            }
            catch
            {
                previousView.gameObject.SetActive(false);
                currentView.gameObject.SetActive(true);
                await currentView.EnterAsync();
                throw;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"[VIEW] Pop failed. Current View remains {CurrentView?.GetType().Name ?? "None"}.", this);
            Debug.LogException(exception, this);
            return false;
        }
        finally
        {
            SetInputBlocked(false);
            _isTransitioning = false;
        }
    }

    private void RegisterInitialView()
    {
        if (initialView == null)
        {
            return;
        }

        initialView.gameObject.SetActive(true);
        if (initialView.ReuseMode == ViewReuseMode.Reuse)
        {
            _viewCache[initialView.GetType()] = initialView;
        }
        _currentView = initialView;
        CurrentViewChanged?.Invoke(_currentView);
        _isTransitioning = true;
        SetInputBlocked(true);
    }

    private async UniTaskVoid EnterInitialViewAsync()
    {
        try
        {
            await initialView.CreateAsync();
            await initialView.EnterAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
        finally
        {
            SetInputBlocked(false);
            _isTransitioning = false;
        }
    }

    private async UniTask<BaseView> LoadViewAsync(Type viewType)
    {
        if (_viewCache.TryGetValue(viewType, out var cachedView))
        {
            ViewSystemLogger.Info($"View instance reused: {viewType.Name}", cachedView);
            return cachedView;
        }

        var prefab = viewPrefabs.Find(candidate => candidate != null && candidate.GetType() == viewType);
        if (prefab == null)
        {
            throw new InvalidOperationException($"View prefab is not registered: {viewType.Name}");
        }

        var instance = Instantiate(prefab, viewRoot);
        instance.name = prefab.name;
        instance.gameObject.SetActive(false);

        try
        {
            await instance.CreateAsync();
            ViewSystemLogger.Info($"View instance created: {viewType.Name} ({instance.ReuseMode})", instance);
            if (instance.ReuseMode == ViewReuseMode.Reuse)
            {
                _viewCache.Add(viewType, instance);
            }

            return instance;
        }
        catch
        {
            Destroy(instance.gameObject);
            throw;
        }
    }

    private void ValidateViewPrefabs()
    {
        var registeredTypes = new HashSet<Type>();
        foreach (var prefab in viewPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("View Prefabs contains an empty entry.", this);
                continue;
            }

            if (!registeredTypes.Add(prefab.GetType()))
            {
                Debug.LogError($"View prefab type is registered more than once: {prefab.GetType().Name}", this);
            }
        }
    }

    private static async UniTask RunBeforeTransitionTasksAsync(Func<UniTask>[] tasks)
    {
        if (tasks == null)
        {
            return;
        }

        foreach (var task in tasks)
        {
            if (task != null)
            {
                await task();
            }
        }
    }

    private static async UniTask RestoreViewAsync(BaseView view)
    {
        if (view == null)
        {
            return;
        }

        view.gameObject.SetActive(true);
        await view.EnterAsync();
    }

    private static async UniTask ReleaseDiscardedViewAsync(
        BaseView discardedView,
        BaseView currentView,
        BaseView previousView)
    {
        if (discardedView == null ||
            discardedView == currentView ||
            discardedView == previousView ||
            discardedView.ReuseMode != ViewReuseMode.Recreate)
        {
            return;
        }

        try
        {
            await discardedView.ReleaseAsync();
            Destroy(discardedView.gameObject);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[VIEW] Failed to release {discardedView.GetType().Name}.", discardedView);
            Debug.LogException(exception, discardedView);
        }
    }

    private void SetInputBlocked(bool blocked)
    {
        if (raycastBlocker != null)
        {
            raycastBlocker.SetActive(blocked);
        }
    }
}
