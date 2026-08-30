using System.Collections.Generic;
using System.Linq;
using NikkeCopy.Client.MVC.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NikkeCopy.Client.Editor
{
    [CustomEditor(typeof(ViewGraph))]
    public sealed class ViewGraphEditor : UnityEditor.Editor
    {
        private readonly List<string> _errors = new();
        private SerializedProperty _initialView;
        private SerializedProperty _nodes;
        private SerializedProperty _edges;

        private void OnEnable()
        {
            _initialView = serializedObject.FindProperty("initialView");
            _nodes = serializedObject.FindProperty("nodes");
            _edges = serializedObject.FindProperty("edges");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("View Graph", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Views에 화면 프리팹을 등록하고, Transitions에 From + Navigation Key → To 이동 규칙을 정의합니다.",
                MessageType.Info);
            EditorGUILayout.PropertyField(_initialView, new GUIContent("Initial View"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.PropertyField(_nodes, new GUIContent("Views"), true);

            EditorGUILayout.Space(8f);
            EditorGUILayout.PropertyField(_edges, new GUIContent("Transitions"), true);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate", GUILayout.Height(28f))) LogValidation();
                if (GUILayout.Button("Sync Canvas", GUILayout.Height(28f))) ViewCanvasSynchronizer.Sync((ViewGraph)target);
            }

            DrawValidationSummary();
        }

        private void DrawValidationSummary()
        {
            var graph = (ViewGraph)target;
            graph.Validate(_errors);
            if (_errors.Count == 0)
            {
                EditorGUILayout.HelpBox("View graph is valid.", MessageType.Info);
                return;
            }

            foreach (var error in _errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        private void LogValidation()
        {
            var graph = (ViewGraph)target;
            graph.Validate(_errors);
            if (_errors.Count == 0) Debug.Log($"ViewGraph '{graph.name}' is valid.", graph);
            else foreach (var error in _errors) Debug.LogError($"ViewGraph: {error}", graph);
        }
    }

    internal static class ViewCanvasSynchronizer
    {
        public static void Sync(ViewGraph graph)
        {
            var errors = new List<string>();
            graph.Validate(errors);
            if (errors.Count > 0)
            {
                foreach (var error in errors) Debug.LogError($"ViewGraph: {error}", graph);
                return;
            }

            var navigator = Object.FindFirstObjectByType<ViewNavigator>();
            if (navigator == null || navigator.ViewRoot == null)
            {
                Debug.LogError("Open a client Scene containing ViewNavigator and ViewRoot before syncing.");
                return;
            }

            Undo.SetCurrentGroupName("Sync View Graph To Canvas");
            var existing = navigator.ViewRoot.GetComponentsInChildren<ViewPrefab>(true).ToDictionary(marker => marker.Key);
            var instances = new Dictionary<ViewKey, GameObject>();

            foreach (var node in graph.Nodes)
            {
                existing.TryGetValue(node.key, out var marker);
                var source = marker == null ? null : PrefabUtility.GetCorrespondingObjectFromSource(marker.gameObject);
                if (marker == null || source != node.prefab)
                {
                    if (marker != null) Undo.DestroyObjectImmediate(marker.gameObject);
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(node.prefab, navigator.ViewRoot);
                    Undo.RegisterCreatedObjectUndo(instance, "Create View Prefab Instance");
                    marker = instance.GetComponent<ViewPrefab>();
                }

                marker.gameObject.name = node.prefab.name;
                marker.gameObject.SetActive(node.key == graph.InitialView);
                instances[node.key] = marker.gameObject;
            }

            foreach (var stale in existing)
                if (!instances.ContainsKey(stale.Key) && stale.Value != null)
                    Undo.DestroyObjectImmediate(stale.Value.gameObject);

            var serialized = new SerializedObject(navigator);
            serialized.FindProperty("graph").objectReferenceValue = graph;
            var bindings = serialized.FindProperty("viewBindings");
            bindings.arraySize = graph.Nodes.Count;
            for (var index = 0; index < graph.Nodes.Count; index++)
            {
                var node = graph.Nodes[index];
                var binding = bindings.GetArrayElementAtIndex(index);
                binding.FindPropertyRelative("key").enumValueIndex = (int)node.key;
                binding.FindPropertyRelative("view").objectReferenceValue = instances[node.key];
            }

            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(navigator.gameObject.scene);
            Debug.Log($"Synced {instances.Count} View prefabs to '{navigator.ViewRoot.name}'.", navigator);
        }
    }
}
