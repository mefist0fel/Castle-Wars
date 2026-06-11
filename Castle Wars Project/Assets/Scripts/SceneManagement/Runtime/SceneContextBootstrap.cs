using UnityEngine;

/// <summary>
/// Automatically registers all scenes that are open when the game starts
/// (cold-start / direct Play from editor, or any bootstrap scene in a build).
/// Removes the need to call <see cref="SceneContext.EnsureCurrentContext"/> manually.
/// </summary>
internal static class SceneContextBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoRegisterScenes()
    {
#if UNITY_EDITOR
        // In the editor, scan the asset database directly — reliable regardless of
        // whether the SceneContext is already referenced by an in-scene MonoBehaviour.
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:SceneContext"))
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var ctx  = UnityEditor.AssetDatabase.LoadAssetAtPath<SceneContext>(path);
            if (ctx != null)
                ctx.EnsureCurrentContext();
        }
#else
        // In builds, SceneContext is always in memory: it is held by a [SerializeField]
        // on a MonoBehaviour in the loaded scene, so FindObjectsOfTypeAll is safe here.
        foreach (var ctx in Resources.FindObjectsOfTypeAll<SceneContext>())
            ctx.EnsureCurrentContext();
#endif
    }
}
