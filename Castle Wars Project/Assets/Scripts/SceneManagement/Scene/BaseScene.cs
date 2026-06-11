using System;
using UnityEngine;

public abstract class BaseScene<TToken> : MonoBehaviour, IScene
{
    [Tooltip("Root GameObjects activated/deactivated with this scene. " +
             "Use 'Refresh Scene Roots' from the context menu to auto-populate.")]
    [SerializeField] private GameObject[] _sceneRoots;

    public virtual string TokenId    => typeof(TToken).Name;
    protected TToken      Token      { get; set; }

    // ── IScene ────────────────────────────────────────────────────────────────

    public void Enter(object tokenPayload)
    {
        Debug.Log($"[BaseScene] Entering '{name}' with token '{tokenPayload}'.");
        Token = (TToken)tokenPayload;
        SetSceneActive(true);
        DoEnter(Token);
    }

    public void Exit()
    {
        DoExit(() =>
        {
            SetSceneActive(false);
            Token = default;
        });
    }

    // ── Extension points ──────────────────────────────────────────────────────

    /// <summary>Called after the scene activates. Override for enter logic.</summary>
    protected virtual void OnEnter(TToken token) { }

    /// <summary>Called at the start of exit, before deactivation. Override for exit logic.</summary>
    protected virtual void OnExit() { }

    /// <summary>
    /// Wraps enter behaviour. Calls <see cref="OnEnter"/> by default.
    /// Override (e.g. in <see cref="AnimatedBaseScene{TToken}"/>) to add animations.
    /// </summary>
    protected virtual void DoEnter(TToken token) => OnEnter(token);

    /// <summary>
    /// Wraps exit behaviour. Must eventually call <paramref name="deactivate"/>
    /// (which deactivates scene roots and clears the token).
    /// Override to add animations before deactivation.
    /// </summary>
    protected virtual void DoExit(Action deactivate) {
        OnExit();
        deactivate();
    }


    protected void SetSceneActive(bool active)
    {
        if (_sceneRoots != null && _sceneRoots.Length > 0)
        {
            foreach (var root in _sceneRoots)
                if (root != null)
                    root.SetActive(active);
        }
        else
        {
            // No curated list — toggle every root in this Unity scene.
            // This ensures cameras, UI, and other roots outside the controller GO
            // are all deactivated/reactivated together.
            foreach (var root in gameObject.scene.GetRootGameObjects())
                root.SetActive(active);
        }
    }

#if UNITY_EDITOR
    private void Reset() => RefreshSceneRoots();

    protected void RefreshSceneRoots()
    {
        var scene = gameObject.scene;
        if (!scene.IsValid())
            return;

        _sceneRoots = scene.GetRootGameObjects();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
