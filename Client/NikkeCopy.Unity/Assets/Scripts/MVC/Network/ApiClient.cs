using System;
using System.Collections;
using System.Text;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;
#endif
using UnityEngine.Networking;

namespace NikkeCopy.Client.Network
{
    public sealed class ApiClient
    {
        public const string DefaultBaseUrl = "http://localhost:5000";
        public static ApiClient Instance { get; } = new ApiClient(DefaultBaseUrl);

        private readonly string _baseUrl;
        private string _bearerToken;

        private ApiClient(string baseUrl)
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

#if UNITY_EDITOR
            EditorApiLogger.LogRequest(
                request.method,
                request.url,
                request.uploadHandler?.data == null
                    ? null
                    : Encoding.UTF8.GetString(request.uploadHandler.data));
#endif
            yield return request.SendWebRequest();

#if UNITY_EDITOR
            EditorApiLogger.LogResponse(
                request.method,
                request.url,
                request.responseCode,
                request.downloadHandler?.text,
                request.result);
#endif

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

#if UNITY_EDITOR
    internal static class EditorApiLogger
    {
        private const string RequestColor = "#57D163";
        private const string ResponseColor = "#FF9F43";
        private static readonly Regex SensitiveJsonValue = new(
            "\\\"(password|accessToken)\\\"\\s*:\\s*\\\"[^\\\"]*\\\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void LogRequest(string method, string url, string body)
        {
            var message = $"<color={RequestColor}><b>[API REQUEST]</b></color> {method} {url}";
            if (!string.IsNullOrWhiteSpace(body))
            {
                message += $"\n{Redact(body)}";
            }

            Debug.Log(message);
        }

        public static void LogResponse(
            string method,
            string url,
            long statusCode,
            string body,
            UnityWebRequest.Result result)
        {
            var message = $"<color={ResponseColor}><b>[API RESPONSE]</b></color> " +
                          $"{method} {url} → {statusCode} ({result})";
            if (!string.IsNullOrWhiteSpace(body))
            {
                message += $"\n{Redact(body)}";
            }

            Debug.Log(message);
        }

        private static string Redact(string json)
        {
            return SensitiveJsonValue.Replace(json, match =>
            {
                var separator = match.Value.IndexOf(':');
                return match.Value.Substring(0, separator + 1) + " \"***\"";
            });
        }
    }
#endif

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
