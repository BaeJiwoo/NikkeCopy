using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

namespace NikkeCopy.Client.Network
{
    public sealed class ApiClient
    {
        public const string DefaultBaseUrl = "http://localhost:5000";

        private readonly string _baseUrl;
        private string _bearerToken;

        public ApiClient(string baseUrl = DefaultBaseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public void SetBearerToken(string token)
        {
            _bearerToken = token;
        }

        public IEnumerator Get(
            string path,
            Action<string> onSuccess,
            Action<ApiError> onError)
        {
            using UnityWebRequest request = UnityWebRequest.Get(BuildUrl(path));
            yield return Send(request, onSuccess, onError);
        }

        public IEnumerator PostJson(
            string path,
            string json,
            Action<string> onSuccess,
            Action<ApiError> onError)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            using UnityWebRequest request = new UnityWebRequest(BuildUrl(path), UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            yield return Send(request, onSuccess, onError);
        }

        private IEnumerator Send(
            UnityWebRequest request,
            Action<string> onSuccess,
            Action<ApiError> onError)
        {
            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrWhiteSpace(_bearerToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_bearerToken}");
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
                yield break;
            }

            onError?.Invoke(new ApiError(
                request.responseCode,
                request.error,
                request.downloadHandler?.text));
        }

        private string BuildUrl(string path)
        {
            return $"{_baseUrl}/{path.TrimStart('/')}";
        }
    }

    public sealed class ApiError
    {
        public ApiError(long statusCode, string message, string responseBody)
        {
            StatusCode = statusCode;
            Message = message;
            ResponseBody = responseBody;
        }

        public long StatusCode { get; }
        public string Message { get; }
        public string ResponseBody { get; }

        public override string ToString()
        {
            return $"HTTP {StatusCode}: {Message}\n{ResponseBody}";
        }
    }
}
