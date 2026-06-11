using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Scene Context/Settings", fileName = "SceneContext")]
public sealed class SceneContext : ScriptableObject
{
    [SerializeField] private List<SceneEntry> _scenes = new();

    [Tooltip("Optional fade-overlay prefab")]
    [SerializeField] private LoadingFader _faderPrefab = null;

#if UNITY_EDITOR
    [Space]
    [Tooltip("Editor only: override which scene Unity opens when you press Play. " +
             "Leave empty to keep the normally-open scene. " +
             "The start scene receives a null token and must handle it gracefully.")]
    [SerializeField] private UnityEditor.SceneAsset _editorStartScene;

    public UnityEditor.SceneAsset EditorStartScene => _editorStartScene;
#endif

    private readonly List<SceneHandle> _activeScenes = new();
    private readonly Dictionary<string, SceneHandle> _handleCache = new();

    private LoadingFader _fader;

    public bool IsShown<TToken>() where TToken : SceneToken {
        foreach (var scene in _activeScenes)
        {
            if (scene.Token is TToken)
                return true;
        }
        return false;
    }

    public void Show(SceneToken token)
    {
        var entry = FindEntry(token.SceneId);
        if (entry == null)
        {
            Debug.LogError($"[SceneContext] No entry for token '{token.SceneId}'. " +
                           "Did you add it to the SceneContext asset?");
            return;
        }

        if (entry.Layer == SceneType.State)
        {
            EnsureFader();
            if (_fader != null)
            {
                _fader.Show(() => AfterFadeIn(entry, token));
                return;
            }
        }

        AfterFadeIn(entry, token);
    }

    public bool SendMessage<T>(T message)
    {
        foreach (var layerType in MessageLayerOrder)
        {
            foreach (var scene in _activeScenes)
            {
                if (scene.Layer != layerType)
                    continue;

                if (scene.Controller is IMessageHandler<T> handler && handler.SendMessage(message))
                    return true;
            }
        }

        return false;
    }

    private static readonly SceneType[] MessageLayerOrder =
    {
        SceneType.Window,
        SceneType.Screen,
        SceneType.State,
        SceneType.Static,
    };

    public void Hide<TToken>() where TToken : SceneToken
    {
        foreach (var scene in _activeScenes)
        {
            if (scene.Token is TToken)
            {
                HideScene(scene);
                _activeScenes.Remove(scene);
                return;
            }
        }
    }

    // ── Bootstrap ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Scans all currently loaded Unity scenes and registers any that contain
    /// an IScene component and have a matching entry in this SceneContext.
    /// Called automatically by <see cref="SceneContextBootstrap"/> on game start.
    /// On a cold-start (direct Play), also calls Enter with a null token so the
    /// scene can initialise itself — scenes that support this must handle null gracefully.
    /// </summary>
    public void EnsureCurrentContext()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene      = SceneManager.GetSceneAt(i);
            var controller = FindController(scene);
            if (controller == null) continue;

            var entry = FindEntry(controller.TokenId);
            if (entry == null) continue;

            bool alreadyTracked = false;
            foreach (var h in _activeScenes)
                if (h.TokenId == entry.TokenId) { alreadyTracked = true; break; }
            if (alreadyTracked) continue;

            var handle = GetOrCreateHandle(entry);
            handle.UnityScene = scene;
            handle.Controller = controller;
            handle.IsLoaded   = true;
            _activeScenes.Add(handle);

            // Cold-start: Token is null → call Enter(null) so the scene can initialise.
            // If a Show() call is already in progress, Token is set to the real token,
            // so we skip here — the LoadAndEnter completed callback will call Enter.
            if (handle.Token == null)
                handle.Controller.Enter(null);

            Debug.Log($"[SceneContext] Registered existing scene '{scene.name}' as '{entry.TokenId}'.");
        }
    }

    // ── Transition steps ──────────────────────────────────────────────────────

    private void AfterFadeIn(SceneEntry entry, SceneToken token)
    {
        HideLowerLayers(entry);

        if (entry.Layer == SceneType.State)
        {
            GC.Collect();
            Resources.UnloadUnusedAssets().completed += _ => LoadAndEnter(entry, token);
        }
        else
        {
            LoadAndEnter(entry, token);
        }
    }

    private void LoadAndEnter(SceneEntry entry, SceneToken token)
    {
        var handle = GetOrCreateHandle(entry);
        handle.Token = token;

        if (handle.IsLoaded)
        {
            if (!_activeScenes.Contains(handle))
                _activeScenes.Add(handle);

            Enter(handle, entry);
            return;
        }

        SceneManager.LoadSceneAsync(handle.SceneName, LoadSceneMode.Additive).completed += _ =>
        {
            handle.UnityScene = SceneManager.GetSceneByName(handle.SceneName);
            // Controller may already be set by EnsureCurrentContext() if Awake() ran during load.
            if (handle.Controller == null)
                handle.Controller = FindController(handle.UnityScene);
            handle.IsLoaded = true;

            if (handle.Controller == null)
                Debug.LogError($"[SceneContext] No IScene found in scene '{handle.SceneName}'.");

            // Guard against duplicate registration from EnsureCurrentContext() during Awake().
            if (!_activeScenes.Contains(handle))
                _activeScenes.Add(handle);

            Enter(handle, entry);
        };
    }

    private void Enter(SceneHandle handle, SceneEntry entry)
    {
        handle.Controller?.Enter(handle.Token);

        if (entry.Layer == SceneType.State)
        {
            SceneManager.SetActiveScene(handle.UnityScene);
            if (_fader != null)
            {
                _fader.Hide();
            }
        }
    }

    private void HideLowerLayers(SceneEntry entry)
    {   // Hide lower layers if needed
        switch (entry.Layer)
        {
            case SceneType.State: HideScreenLayers(SceneType.State, SceneType.Screen, SceneType.Window); break;
            case SceneType.Screen: HideScreenLayers(SceneType.Screen, SceneType.Window); break;
            case SceneType.Window: break;
            case SceneType.Static: break;
            default: break;
        }
    }

    private void HideScreenLayers(params SceneType[] layersToHide)
    {
        if (layersToHide == null || layersToHide.Length == 0)
            return;
        foreach (var scene in _activeScenes)
        {
            if (layersToHide.Contains(scene.Layer))
            {
                HideScene(scene);
            }
        }
        _activeScenes.RemoveAll(scene => layersToHide.Contains(scene.Layer));
    }

    private void HideScene(SceneHandle handle)
    {
        handle.Controller?.Exit();
        handle.Token = null;
        if (!handle.Cached)
        {
            UnloadScene(handle);
            _handleCache.Remove(handle.TokenId);
        }
    }

    private SceneEntry FindEntry(string tokenId)
    {
        foreach (var scene in _scenes)
            if (scene.TokenId == tokenId)
                return scene;
        return null;
    }

    private SceneHandle GetOrCreateHandle(SceneEntry entry)
    {
        if (_handleCache.TryGetValue(entry.TokenId, out var handle))
            return handle;
        var newHandle = new SceneHandle(entry);
        _handleCache[entry.TokenId] = newHandle;
        return newHandle;
    }

    private static void UnloadScene(SceneHandle handle)
    {
        if (!handle.IsLoaded)
            return;
        handle.IsLoaded = false;
        handle.UnityScene = default;
        handle.Controller = null;
        SceneManager.UnloadSceneAsync(handle.SceneName);
    }

    private static IScene FindController(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var sceneController = root.GetComponentInChildren<IScene>(true);
            if (sceneController != null)
                return sceneController;
        }
        return null;
    }

    private void EnsureFader()
    {
        if (_faderPrefab == null || _fader != null)
            return;

        _fader = Instantiate(_faderPrefab);
        DontDestroyOnLoad(_fader.gameObject);
    }
}
