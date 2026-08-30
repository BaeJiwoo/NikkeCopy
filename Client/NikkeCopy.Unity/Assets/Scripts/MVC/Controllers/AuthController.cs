using NikkeCopy.Client.MVC.Models;
using NikkeCopy.Client.MVC.Navigation;
using NikkeCopy.Client.MVC.Views;
using NikkeCopy.Client.Network.Auth;
using UnityEngine;

namespace NikkeCopy.Client.MVC.Controllers
{
    public sealed class AuthController : MonoBehaviour
    {
        [SerializeField] private AuthModel model;
        [SerializeField] private AuthView view;
        [SerializeField] private ViewNavigator navigator;

        private void OnEnable()
        {
            if (navigator == null) navigator = FindFirstObjectByType<ViewNavigator>();
            view.LoginRequested += Login;
            view.RegisterRequested += Register;
        }

        private void Start()
        {
            if (model.IsAuthenticated)
            {
                view.ShowStatus("저장된 로그인 정보가 있습니다.");
            }
        }

        private void OnDisable()
        {
            view.LoginRequested -= Login;
            view.RegisterRequested -= Register;
        }

        private void Login(string username, string password)
        {
            if (!Validate(username, password)) return;
            view.SetInteractable(false);
            view.ShowStatus("로그인 중...");
            StartCoroutine(model.Login(username, password, Complete, Fail));
        }

        private void Register(string username, string password)
        {
            if (!Validate(username, password)) return;
            view.SetInteractable(false);
            view.ShowStatus("계정 생성 중...");
            StartCoroutine(model.Register(username, password, Complete, Fail));
        }

        private bool Validate(string username, string password)
        {
            if (username.Length >= 3 && username.Length <= 30 &&
                password.Length >= 8 && password.Length <= 128)
            {
                return true;
            }

            view.ShowStatus("아이디는 3~30자, 비밀번호는 8~128자로 입력하세요.", true);
            return false;
        }

        private void Complete(AuthResponse response)
        {
            view.ShowStatus($"{response.username} 로그인 성공");
            navigator.Navigate(NavigationKey.ShowMain);
        }

        private void Fail(string errorCode)
        {
            view.SetInteractable(true);
            view.ShowStatus(ToMessage(errorCode), true);
        }

        private static string ToMessage(string errorCode)
        {
            return errorCode switch
            {
                "invalid_credentials" => "아이디 또는 비밀번호가 올바르지 않습니다.",
                "invalid_credentials_format" => "입력 형식을 확인하세요.",
                "username_unavailable" => "사용할 수 없는 아이디입니다.",
                "network_unavailable" => "서버에 연결할 수 없습니다.",
                _ => $"인증 요청에 실패했습니다. ({errorCode})"
            };
        }
    }
}
