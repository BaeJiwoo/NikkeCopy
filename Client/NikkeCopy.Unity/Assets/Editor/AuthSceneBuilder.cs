using NikkeCopy.Client.MVC.Controllers;
using NikkeCopy.Client.MVC.Models;
using NikkeCopy.Client.MVC.Navigation;
using NikkeCopy.Client.MVC.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace NikkeCopy.Client.Editor
{
    public static class AuthSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/AuthScene.unity";
        private const string GraphPath = "Assets/Settings/ClientViewGraph.asset";
        private const string ViewPrefabFolder = "Assets/GameResources/Prefabs/UI/Views";
        private const string AuthPrefabPath = ViewPrefabFolder + "/AuthView.prefab";
        private const string MainPrefabPath = ViewPrefabFolder + "/MainView.prefab";

        [MenuItem("NikkeCopy/Build Client View Scene")]
        public static void Build()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolderRecursive(ViewPrefabFolder);
            BuildAuthScene();
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log($"Client view scene created at {ScenePath}");
        }

        private static void BuildAuthScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cameraObject.GetComponent<Camera>().backgroundColor = new Color(0.025f, 0.035f, 0.065f);

            var system = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            system.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var canvasObject = new GameObject("AuthCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var background = CreateImage("Background", canvasObject.transform, new Color(0.025f, 0.035f, 0.065f, 1f));
            Stretch(background.rectTransform);

            var viewRoot = new GameObject("ViewRoot", typeof(RectTransform));
            viewRoot.transform.SetParent(background.transform, false);
            Stretch(viewRoot.GetComponent<RectTransform>());

            var authRoot = new GameObject("AuthView", typeof(RectTransform), typeof(ViewPrefab));
            authRoot.transform.SetParent(viewRoot.transform, false);
            Stretch(authRoot.GetComponent<RectTransform>());
            SetEnum(authRoot.GetComponent<ViewPrefab>(), "key", (int)ViewKey.Auth);

            var panel = CreateImage("LoginPanel", authRoot.transform, new Color(0.07f, 0.09f, 0.15f, 0.98f));
            SetCentered(panel.rectTransform, new Vector2(520f, 500f), Vector2.zero);

            CreateText("Title", panel.transform, "NIKKE COPY", 42, FontStyle.Bold, new Vector2(0f, 185f), new Vector2(440f, 70f));
            CreateText("Subtitle", panel.transform, "ACCOUNT LOGIN", 18, FontStyle.Normal, new Vector2(0f, 140f), new Vector2(440f, 35f));

            var username = CreateInput("UsernameInput", panel.transform, "아이디", false, new Vector2(0f, 65f));
            var password = CreateInput("PasswordInput", panel.transform, "비밀번호", true, new Vector2(0f, -5f));
            var login = CreateButton("LoginButton", panel.transform, "로그인", new Vector2(0f, -85f), new Color(0.16f, 0.42f, 0.78f));
            var register = CreateButton("RegisterButton", panel.transform, "회원가입", new Vector2(0f, -150f), new Color(0.20f, 0.24f, 0.34f));
            var status = CreateText("StatusText", panel.transform, string.Empty, 16, FontStyle.Normal, new Vector2(0f, -210f), new Vector2(440f, 50f));

            var authObject = new GameObject("AuthFeature", typeof(AuthModel), typeof(AuthView), typeof(AuthController));
            authObject.transform.SetParent(authRoot.transform, false);
            var model = authObject.GetComponent<AuthModel>();
            var view = authObject.GetComponent<AuthView>();
            var controller = authObject.GetComponent<AuthController>();

            var mainRoot = new GameObject("MainView", typeof(RectTransform), typeof(ViewPrefab));
            mainRoot.transform.SetParent(viewRoot.transform, false);
            Stretch(mainRoot.GetComponent<RectTransform>());
            SetEnum(mainRoot.GetComponent<ViewPrefab>(), "key", (int)ViewKey.Main);
            CreateText("Title", mainRoot.transform, "NIKKE COPY", 48, FontStyle.Bold, new Vector2(0f, 40f), new Vector2(700f, 90f));
            CreateText("WelcomeText", mainRoot.transform, "메인 화면", 26, FontStyle.Normal, new Vector2(0f, -35f), new Vector2(700f, 60f));
            var authButton = CreateButton("AuthViewButton", mainRoot.transform, "로그인 화면으로", new Vector2(0f, -120f), new Color(0.20f, 0.24f, 0.34f));

            var navigationButton = authButton.gameObject.AddComponent<ViewNavigationButton>();
            SetReference(navigationButton, "button", authButton);
            SetEnum(navigationButton, "buttonKey", (int)NavigationKey.ShowAuth);

            PrefabUtility.SaveAsPrefabAssetAndConnect(authRoot, AuthPrefabPath, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAssetAndConnect(mainRoot, MainPrefabPath, InteractionMode.AutomatedAction);
            var authPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AuthPrefabPath);
            var mainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainPrefabPath);
            var graph = CreateOrUpdateViewGraph(authPrefab, mainPrefab);

            var navigationObject = new GameObject("ViewNavigation", typeof(ViewNavigator));
            navigationObject.transform.SetParent(canvasObject.transform, false);
            var navigator = navigationObject.GetComponent<ViewNavigator>();
            SetViewBindings(navigator, graph, viewRoot.transform, authRoot, mainRoot);
            SetReference(navigationButton, "navigator", navigator);

            SetReference(view, "usernameInput", username);
            SetReference(view, "passwordInput", password);
            SetReference(view, "loginButton", login);
            SetReference(view, "registerButton", register);
            SetReference(view, "statusText", status);
            SetReference(controller, "model", model);
            SetReference(controller, "view", view);
            SetReference(controller, "navigator", navigator);

            mainRoot.SetActive(false);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static InputField CreateInput(string name, Transform parent, string placeholder, bool password, Vector2 position)
        {
            var image = CreateImage(name, parent, new Color(0.11f, 0.14f, 0.22f));
            SetCentered(image.rectTransform, new Vector2(420f, 54f), position);
            var input = image.gameObject.AddComponent<InputField>();
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.lineType = InputField.LineType.SingleLine;

            var placeholderText = CreateText("Placeholder", image.transform, placeholder, 18, FontStyle.Italic, Vector2.zero, new Vector2(372f, 50f));
            placeholderText.color = new Color(0.55f, 0.6f, 0.7f);
            var valueText = CreateText("Text", image.transform, string.Empty, 18, FontStyle.Normal, Vector2.zero, new Vector2(372f, 50f));
            valueText.alignment = TextAnchor.MiddleLeft;
            placeholderText.alignment = TextAnchor.MiddleLeft;
            input.placeholder = placeholderText;
            input.textComponent = valueText;
            return input;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Color color)
        {
            var image = CreateImage(name, parent, color);
            SetCentered(image.rectTransform, new Vector2(420f, 52f), position);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateText("Label", image.transform, label, 19, FontStyle.Bold, Vector2.zero, new Vector2(400f, 48f));
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int size, FontStyle style, Vector2 position, Vector2 dimensions)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var text = gameObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            SetCentered(text.rectTransform, dimensions, position);
            return text;
        }

        private static void SetCentered(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).enumValueIndex = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ViewGraph CreateOrUpdateViewGraph(GameObject authPrefab, GameObject mainPrefab)
        {
            var graph = AssetDatabase.LoadAssetAtPath<ViewGraph>(GraphPath);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<ViewGraph>();
                AssetDatabase.CreateAsset(graph, GraphPath);
            }

            var serialized = new SerializedObject(graph);
            serialized.FindProperty("initialView").enumValueIndex = (int)ViewKey.Auth;

            var nodes = serialized.FindProperty("nodes");
            nodes.arraySize = 2;
            SetNode(nodes.GetArrayElementAtIndex(0), ViewKey.Auth, authPrefab);
            SetNode(nodes.GetArrayElementAtIndex(1), ViewKey.Main, mainPrefab);

            var edges = serialized.FindProperty("edges");
            edges.arraySize = 2;
            SetEdge(edges.GetArrayElementAtIndex(0), ViewKey.Auth, NavigationKey.ShowMain, ViewKey.Main);
            SetEdge(edges.GetArrayElementAtIndex(1), ViewKey.Main, NavigationKey.ShowAuth, ViewKey.Auth);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(graph);
            return graph;
        }

        private static void SetNode(SerializedProperty node, ViewKey key, GameObject prefab)
        {
            node.FindPropertyRelative("key").enumValueIndex = (int)key;
            node.FindPropertyRelative("prefab").objectReferenceValue = prefab;
        }

        private static void SetEdge(SerializedProperty edge, ViewKey from, NavigationKey navigation, ViewKey to)
        {
            edge.FindPropertyRelative("from").enumValueIndex = (int)from;
            edge.FindPropertyRelative("navigation").enumValueIndex = (int)navigation;
            edge.FindPropertyRelative("to").enumValueIndex = (int)to;
        }

        private static void SetViewBindings(
            ViewNavigator navigator,
            ViewGraph graph,
            Transform viewRoot,
            GameObject authView,
            GameObject mainView)
        {
            var serialized = new SerializedObject(navigator);
            serialized.FindProperty("graph").objectReferenceValue = graph;
            serialized.FindProperty("viewRoot").objectReferenceValue = viewRoot;
            var bindings = serialized.FindProperty("viewBindings");
            bindings.arraySize = 2;

            var authBinding = bindings.GetArrayElementAtIndex(0);
            authBinding.FindPropertyRelative("key").enumValueIndex = (int)ViewKey.Auth;
            authBinding.FindPropertyRelative("view").objectReferenceValue = authView;

            var mainBinding = bindings.GetArrayElementAtIndex(1);
            mainBinding.FindPropertyRelative("key").enumValueIndex = (int)ViewKey.Main;
            mainBinding.FindPropertyRelative("view").objectReferenceValue = mainView;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void EnsureFolderRecursive(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }

        private static void SetBuildScenes()
        {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
