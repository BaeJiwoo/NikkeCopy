using System;
using System.Collections;
using UnityEngine;

namespace NikkeCopy.Client.Network.Auth
{
    [Serializable]
    public sealed class AuthRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    public sealed class AuthResponse
    {
        public bool isSuccess;
        public string accessToken;
        public string expiresAt;
        public long accountId;
        public string username;
        public string errorCode;
    }

    public sealed class AuthApi
    {
        private readonly ApiClient _client;

        public AuthApi(ApiClient client)
        {
            _client = client;
        }

        public IEnumerator Login(
            string username,
            string password,
            Action<AuthResponse> onSuccess,
            Action<ApiError> onError)
        {
            return Send("api/auth/login", username, password, onSuccess, onError);
        }

        public IEnumerator Register(
            string username,
            string password,
            Action<AuthResponse> onSuccess,
            Action<ApiError> onError)
        {
            return Send("api/auth/register", username, password, onSuccess, onError);
        }

        private IEnumerator Send(
            string path,
            string username,
            string password,
            Action<AuthResponse> onSuccess,
            Action<ApiError> onError)
        {
            var request = new AuthRequest { username = username, password = password };
            yield return _client.PostJson(
                path,
                JsonUtility.ToJson(request),
                json => onSuccess?.Invoke(JsonUtility.FromJson<AuthResponse>(json)),
                onError);
        }
    }
}
