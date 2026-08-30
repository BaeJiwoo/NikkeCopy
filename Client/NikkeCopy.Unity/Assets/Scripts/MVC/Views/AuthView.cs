using System;
using UnityEngine;
using UnityEngine.UI;

namespace NikkeCopy.Client.MVC.Views
{
    public sealed class AuthView : MonoBehaviour
    {
        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Text statusText;

        public event Action<string, string> LoginRequested;
        public event Action<string, string> RegisterRequested;

        private void Awake()
        {
            passwordInput.contentType = InputField.ContentType.Password;
            loginButton.onClick.AddListener(() => LoginRequested?.Invoke(
                usernameInput.text.Trim(), passwordInput.text));
            registerButton.onClick.AddListener(() => RegisterRequested?.Invoke(
                usernameInput.text.Trim(), passwordInput.text));
        }

        public void SetInteractable(bool interactable)
        {
            usernameInput.interactable = interactable;
            passwordInput.interactable = interactable;
            loginButton.interactable = interactable;
            registerButton.interactable = interactable;
        }

        public void ShowStatus(string message, bool isError = false)
        {
            statusText.text = message;
            statusText.color = isError
                ? new Color(1f, 0.35f, 0.35f)
                : new Color(0.75f, 0.9f, 1f);
        }
    }
}
