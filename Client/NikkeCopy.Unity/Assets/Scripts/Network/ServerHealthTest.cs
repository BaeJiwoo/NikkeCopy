using System;
using UnityEngine;

namespace NikkeCopy.Client.Network
{
    public sealed class ServerHealthTest : MonoBehaviour
    {
        [SerializeField] private bool requestOnStart = true;

        private readonly ApiClient _apiClient = new ApiClient();

        private void Start()
        {
            if (requestOnStart)
            {
                CheckHealth();
            }
        }

        [ContextMenu("Check Server Health")]
        public void CheckHealth()
        {
            StartCoroutine(_apiClient.Get(
                "/api/health",
                response => Debug.Log($"Server health: {response}"),
                error => Debug.LogError($"Server health request failed: {error}")));
        }
    }
}
