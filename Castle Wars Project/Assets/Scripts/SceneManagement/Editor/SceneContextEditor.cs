#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(SceneContext))]
public sealed class SceneContextEditor : Editor
{
    private SerializedProperty _scenesProp;
    private SerializedProperty _fadeProp;
    private SerializedProperty _editorStartSceneProp;

    // ── Play-mode start scene hook ────────────────────────────────────────────

    [InitializeOnLoadMethod]
    private static void SetupPlayModeHook()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;

        EditorSceneManager.playModeStartScene = FindEditorStartScene();
    }

    private static SceneAsset FindEditorStartScene()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:SceneContext"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var ctx  = AssetDatabase.LoadAssetAtPath<SceneContext>(path);
            if (ctx != null && ctx.EditorStartScene != null)
                return ctx.EditorStartScene;
        }
        return null; // null = use the currently-open scene (Unity default)
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        _scenesProp           = serializedObject.FindProperty("_scenes");
        _fadeProp             = serializedObject.FindProperty("_faderPrefab");
        _editorStartSceneProp = serializedObject.FindProperty("_editorStartScene");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_fadeProp);

        // ── Editor start scene ────────────────────────────────────────────────
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_editorStartSceneProp,
            new GUIContent("Start Scene Override",
                "When set, Unity will open this scene on Play instead of the currently-open scene. " +
                "The scene receives a null token and must handle it gracefully. Editor-only."));

        DrawEditorStartSceneWarning();

        // ── Scenes ────────────────────────────────────────────────────────────
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

        for (int i = 0; i < _scenesProp.arraySize; i++)
            DrawEntry(i);

        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ Add Scene", GUILayout.Height(22)))
                _scenesProp.InsertArrayElementAtIndex(_scenesProp.arraySize);

            if (GUILayout.Button("Scan All Tokens", GUILayout.Height(22)))
                ScanAllTokens();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEditorStartSceneWarning()
    {
        var editorScene = _editorStartSceneProp.objectReferenceValue as SceneAsset;
        if (editorScene == null) return;

        // Compare against Build Settings [0] (the production start scene).
        var buildScenes = EditorBuildSettings.scenes;
        string productionPath = buildScenes.Length > 0 ? buildScenes[0].path : null;
        string editorPath     = AssetDatabase.GetAssetPath(editorScene);

        if (productionPath == null)
        {
            EditorGUILayout.HelpBox(
                "No scenes in Build Settings — cannot determine the production start scene.",
                MessageType.Warning);
        }
        else if (editorPath != productionPath)
        {
            string productionName = Path.GetFileNameWithoutExtension(productionPath);
            EditorGUILayout.HelpBox(
                $"Editor start scene is \"{editorScene.name}\" but the production start scene is \"{productionName}\". " +
                "Press Play will not reflect the real game start.",
                MessageType.Warning);
        }
    }

    private void DrawEntry(int index)
    {
        var elem        = _scenesProp.GetArrayElementAtIndex(index);
        var assetProp   = elem.FindPropertyRelative("SceneAsset");
        var nameProp    = elem.FindPropertyRelative("SceneName");
        var tokenProp   = elem.FindPropertyRelative("TokenId");
        var layerProp   = elem.FindPropertyRelative("Layer");
        var cachedProp  = elem.FindPropertyRelative("Cached");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Header row: index + remove button
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"[{index}]  {tokenProp.stringValue}", EditorStyles.boldLabel);
            if (GUILayout.Button("✕", GUILayout.Width(22)))
            {
                _scenesProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndVertical();
                return;
            }
        }

        // Scene asset slot
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(assetProp, new GUIContent("Scene Asset"));
        if (EditorGUI.EndChangeCheck())
        {
            var asset = assetProp.objectReferenceValue as SceneAsset;
            if (asset != null)
            {
                nameProp.stringValue = asset.name;
                if (string.IsNullOrEmpty(tokenProp.stringValue))
                    tokenProp.stringValue = asset.name; // placeholder until scan
                EnsureInBuildSettings(asset);
            }
        }

        EditorGUILayout.PropertyField(nameProp,   new GUIContent("Scene Name"));
        EditorGUILayout.PropertyField(tokenProp,  new GUIContent("Token ID"));
        EditorGUILayout.PropertyField(layerProp,  new GUIContent("Layer"));
        EditorGUILayout.PropertyField(cachedProp, new GUIContent("Cached"));

        // Utility buttons
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scan Token"))
                ScanToken(assetProp, nameProp, tokenProp);

            if (GUILayout.Button("Add to Build"))
            {
                var asset = assetProp.objectReferenceValue as SceneAsset;
                if (asset != null) EnsureInBuildSettings(asset);
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void ScanToken(SerializedProperty assetProp, SerializedProperty nameProp, SerializedProperty tokenProp)
    {
        var asset = assetProp.objectReferenceValue as SceneAsset;
        if (asset == null) { Debug.LogWarning("[SceneContextEditor] No scene asset assigned."); return; }

        string path = AssetDatabase.GetAssetPath(asset);
        var scene   = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

        string found = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var s = root.GetComponentInChildren<IScene>(true);
            if (s != null) { found = s.TokenId; break; }
        }

        EditorSceneManager.CloseScene(scene, true);

        if (found != null)
        {
            tokenProp.stringValue = found;
            nameProp.stringValue  = asset.name;
            Debug.Log($"[SceneContextEditor] Token scanned: '{found}' from {asset.name}");
        }
        else
        {
            Debug.LogWarning($"[SceneContextEditor] No IScene found in {asset.name}.");
        }
    }

    private void ScanAllTokens()
    {
        for (int i = 0; i < _scenesProp.arraySize; i++)
        {
            var elem      = _scenesProp.GetArrayElementAtIndex(i);
            var assetProp = elem.FindPropertyRelative("SceneAsset");
            var nameProp  = elem.FindPropertyRelative("SceneName");
            var tokenProp = elem.FindPropertyRelative("TokenId");
            ScanToken(assetProp, nameProp, tokenProp);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private static void EnsureInBuildSettings(SceneAsset asset)
    {
        string path   = AssetDatabase.GetAssetPath(asset);
        var    scenes = EditorBuildSettings.scenes;

        foreach (var s in scenes)
            if (s.path == path) return; // already present

        var newList = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newList, 0);
        newList[scenes.Length] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = newList;
        Debug.Log($"[SceneContextEditor] Added '{asset.name}' to Build Settings.");
    }
}
#endif