using System;
using System.Collections;
using NikkeCopy.Client.Network;
using NikkeCopy.Client.Network.Auth;
using UnityEngine;

namespace NikkeCopy.Client.MVC.Models
{
    public sealed class AuthModel : MonoBehaviour
    {
        private const string TokenKey = "auth.access_token";
        private ApiClient _client;
        private AuthApi _authApi;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);
        public string AccessToken { get; private set; }

        private void Awake()
        {
            _client = ApiClient.Instance;
            _authApi = new AuthApi(_client);
            AccessToken = PlayerPrefs.GetString(TokenKey, string.Empty);
            _client.SetBearerToken(AccessToken);
        }

        public IEnumerator Login(
            string username,
            string password,
            Action<AuthResponse> onSuccess,
            Action<string> onError)
        {
            yield return _authApi.Login(
                username,
                password,
                response => CompleteAuthentication(response, onSuccess, onError),
                error => onError?.Invoke(ReadError(error)));
        }

        public IEnumerator Register(
            string username,
            string password,
            Action<AuthResponse> onSuccess,
            Action<string> onError)
        {
            yield return _authApi.Register(
                username,
                password,
                response => CompleteAuthentication(response, onSuccess, onError),
                error => onError?.Invoke(ReadError(error)));
        }

        public void Logout()
        {
            AccessToken = string.Empty;
            _client.SetBearerToken(string.Empty);
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.Save();
        }

        private void CompleteAuthentication(
            AuthResponse response,
            Action<AuthResponse> onSuccess,
            Action<string> onError)
        {
            if (response == null || !response.isSuccess || string.IsNullOrWhiteSpace(response.accessToken))
            {
                onError?.Invoke(response?.errorCode ?? "invalid_server_response");
                return;
            }

            AccessToken = response.accessToken;
            _client.SetBearerToken(AccessToken);
            PlayerPrefs.SetString(TokenKey, AccessToken);
            PlayerPrefs.Save();
            onSuccess?.Invoke(response);
        }

        private static string ReadError(ApiError error)
        {
            if (!string.IsNullOrWhiteSpace(error.ResponseBody))
            {
                var response = JsonUtility.FromJson<AuthResponse>(error.ResponseBody);
                if (response != null && !string.IsNullOrWhiteSpace(response.errorCode))
                {
                    return response.errorCode;
                }
            }

            return error.StatusCode == 0 ? "network_unavailable" : $"http_{error.StatusCode}";
        }
    }
}
