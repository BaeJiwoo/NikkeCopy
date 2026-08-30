using System;
using System.Collections.Generic;
using UnityEngine;

namespace NikkeCopy.Client.MVC.Navigation
{
    [CreateAssetMenu(fileName = "ViewGraph", menuName = "NikkeCopy/UI/View Graph")]
    public sealed class ViewGraph : ScriptableObject
    {
        [Serializable]
        public sealed class ViewNode
        {
            public ViewKey key = ViewKey.None;
            public GameObject prefab = null;
        }

        [Serializable]
        public sealed class ViewEdge
        {
            public ViewKey from = ViewKey.None;
            public NavigationKey navigation = NavigationKey.None;
            public ViewKey to = ViewKey.None;
        }

        [SerializeField] private ViewKey initialView = ViewKey.Auth;
        [SerializeField] private List<ViewNode> nodes = new();
        [SerializeField] private List<ViewEdge> edges = new();

        private readonly Dictionary<(ViewKey, NavigationKey), ViewKey> _edgeIndex = new();

        public ViewKey InitialView => initialView;
        public IReadOnlyList<ViewNode> Nodes => nodes;
        public IReadOnlyList<ViewEdge> Edges => edges;

        private void OnEnable() => BuildEdgeIndex();

        public bool TryResolve(ViewKey from, NavigationKey navigation, out ViewKey destination)
        {
            BuildEdgeIndex();
            return _edgeIndex.TryGetValue((from, navigation), out destination);
        }

        public void Validate(List<string> errors)
        {
            errors.Clear();
            var nodeKeys = new HashSet<ViewKey>();
            var prefabs = new HashSet<GameObject>();

            foreach (var node in nodes)
            {
                if (node.key == ViewKey.None) errors.Add("A view node has no key.");
                else if (!nodeKeys.Add(node.key)) errors.Add($"Duplicate view node: {node.key}");

                if (node.prefab == null) errors.Add($"View prefab is missing: {node.key}");
                else
                {
                    if (!prefabs.Add(node.prefab)) errors.Add($"A view prefab is assigned more than once: {node.prefab.name}");
                    var marker = node.prefab.GetComponent<ViewPrefab>();
                    if (marker == null || marker.Key != node.key)
                        errors.Add($"Prefab marker does not match node {node.key}: {node.prefab.name}");
                }
            }

            if (!nodeKeys.Contains(initialView)) errors.Add($"Initial view is not registered as a node: {initialView}");

            var edgeKeys = new HashSet<(ViewKey, NavigationKey)>();
            foreach (var edge in edges)
            {
                if (!nodeKeys.Contains(edge.from) || !nodeKeys.Contains(edge.to))
                    errors.Add($"Edge references a missing node: {edge.from} -> {edge.to}");
                if (edge.navigation == NavigationKey.None)
                    errors.Add($"Edge has no navigation key: {edge.from} -> {edge.to}");
                else if (!edgeKeys.Add((edge.from, edge.navigation)))
                    errors.Add($"Duplicate edge: {edge.from} + {edge.navigation}");
            }

            ValidateReachability(nodeKeys, errors);
        }

        private void BuildEdgeIndex()
        {
            _edgeIndex.Clear();
            foreach (var edge in edges) _edgeIndex.TryAdd((edge.from, edge.navigation), edge.to);
        }

        private void ValidateReachability(HashSet<ViewKey> nodeKeys, List<string> errors)
        {
            if (!nodeKeys.Contains(initialView)) return;
            var reachable = new HashSet<ViewKey> { initialView };
            var queue = new Queue<ViewKey>();
            queue.Enqueue(initialView);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in edges)
                    if (edge.from == current && nodeKeys.Contains(edge.to) && reachable.Add(edge.to)) queue.Enqueue(edge.to);
            }
            foreach (var node in nodeKeys)
                if (!reachable.Contains(node)) errors.Add($"View is unreachable from {initialView}: {node}");
        }
    }
}
