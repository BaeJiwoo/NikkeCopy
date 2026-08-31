using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class ViewPushButton : MonoBehaviour
{
    [SerializeField] private BaseView targetView;

    private Button _button;
    private ViewManager _manager;

    public BaseView TargetView => targetView;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(Navigate);
        BindManager();
    }

    private void Start()
    {
        BindManager();
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(Navigate);
        if (_manager != null)
        {
            _manager.CurrentViewChanged -= RefreshInteractable;
            _manager = null;
        }
    }

    private void Navigate()
    {
        if (targetView == null)
        {
            Debug.LogError("Target View is not assigned.", this);
            return;
        }

        BindManager();
        if (_manager == null)
        {
            Debug.LogError("ViewManager is not available.", this);
            return;
        }

        _manager.PushAsync(targetView).Forget();
    }

    private void BindManager()
    {
        var manager = ViewManager.Instance;
        if (_manager == manager)
        {
            RefreshInteractable(manager?.CurrentView);
            return;
        }

        if (_manager != null)
        {
            _manager.CurrentViewChanged -= RefreshInteractable;
        }

        _manager = manager;
        if (_manager != null)
        {
            _manager.CurrentViewChanged += RefreshInteractable;
        }

        RefreshInteractable(_manager?.CurrentView);
    }

    private void RefreshInteractable(BaseView currentView)
    {
        if (_button == null)
        {
            return;
        }

        _button.interactable = targetView != null &&
                               (currentView == null || targetView.GetType() != currentView.GetType());
    }
}
