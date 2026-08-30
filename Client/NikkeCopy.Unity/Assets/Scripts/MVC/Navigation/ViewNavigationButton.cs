using UnityEngine;
using UnityEngine.UI;

namespace NikkeCopy.Client.MVC.Navigation
{
    [RequireComponent(typeof(Button))]
    public sealed class ViewNavigationButton : MonoBehaviour
    {
        [SerializeField] private NavigationKey buttonKey;
        [SerializeField] private ViewNavigator navigator;
        [SerializeField] private Button button;

        public NavigationKey ButtonKey => buttonKey;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (navigator == null) navigator = FindFirstObjectByType<ViewNavigator>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(Navigate);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(Navigate);
        }

        private void Navigate()
        {
            if (navigator != null) navigator.Navigate(buttonKey);
        }
    }
}
