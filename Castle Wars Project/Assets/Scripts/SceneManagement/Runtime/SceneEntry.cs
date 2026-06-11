using System;
using UnityEngine;

[Serializable]
public sealed class SceneEntry
{
    [Tooltip("Token ID that identifies this scene. Must match the IScene.SceneTokenId " +
             "of the root component in the Unity scene. Auto-filled by the editor inspector.")]
    public string TokenId = string.Empty;

    [Tooltip("Unity scene name (file name without .unity). Used by SceneManager.LoadSceneAsync.")]
    public string SceneName = string.Empty;

    [Tooltip("Stacking layer — controls how transitions interact with other scenes.")]
    public SceneType Layer = SceneType.Window;

    [Tooltip("Keep the scene loaded (hidden) after Exit instead of unloading it. " +
             "Speeds up repeated opens at the cost of memory.")]
    public bool Cached = false;

#if UNITY_EDITOR
    [Tooltip("Drag a scene asset here. The editor will auto-fill TokenId and SceneName.")]
    public UnityEditor.SceneAsset SceneAsset;
#endif
}
