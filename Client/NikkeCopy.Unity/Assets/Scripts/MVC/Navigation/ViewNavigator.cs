using System;
using System.Collections.Generic;
using UnityEngine;

namespace NikkeCopy.Client.MVC.Navigation
{
    public sealed class ViewNavigator : MonoBehaviour
    {
        [Serializable]
        private sealed class ViewBinding
        {
            public ViewKey key = ViewKey.None;
            public GameObject view = null;
        }

        [SerializeField] private ViewGraph graph;
        [SerializeField] private Transform viewRoot;
        [SerializeField] private List<ViewBinding> viewBindings = new();

        private readonly Dictionary<ViewKey, GameObject> _views = new();

        public ViewKey CurrentView { get; private set; }
        public Transform ViewRoot => viewRoot;

        private void Awake()
        {
            if (graph == null)
            {
                Debug.LogError("ViewNavigator requires a ViewGraph.", this);
                return;
            }

            var graphErrors = new List<string>();
            graph.Validate(graphErrors);
            foreach (var error in graphErrors)
            {
                Debug.LogError($"ViewGraph: {error}", graph);
            }

            foreach (var binding in viewBindings)
            {
                if (binding.key == ViewKey.None || binding.view == null)
                {
                    Debug.LogError("ViewNavigator contains an invalid view binding.", this);
                    continue;
                }

                if (!_views.TryAdd(binding.key, binding.view))
                {
                    Debug.LogError($"Duplicate view binding: {binding.key}", this);
                }

                binding.view.SetActive(false);
            }

            Show(graph.InitialView);
        }

        public bool Navigate(NavigationKey key)
        {
            if (!graph.TryResolve(CurrentView, key, out var destination))
            {
                Debug.LogError($"No graph edge for: {CurrentView} + {key}", this);
                return false;
            }

            return Show(destination);
        }

        private bool Show(ViewKey key)
        {
            if (!_views.TryGetValue(key, out var target))
            {
                Debug.LogError($"No Scene view is bound to: {key}", this);
                return false;
            }

            foreach (var view in _views.Values)
            {
                view.SetActive(view == target);
            }

            CurrentView = key;
            return true;
        }
    }
}
