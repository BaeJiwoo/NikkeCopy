using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class BackViewButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(GoBack);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(GoBack);
    }

    private void GoBack()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.PopAsync().Forget();
        }
    }
}
