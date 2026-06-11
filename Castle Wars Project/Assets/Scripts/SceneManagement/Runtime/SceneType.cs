/// <summary>
/// The stacking layer a scene belongs to.
/// Determines how SceneContext handles it during transitions.
/// </summary>
public enum SceneType
{
    /// <summary>
    /// Bottom layer. Exclusive state machine: showing a new State exits all
    /// Windows, the current Screen, and the current State first.
    /// </summary>
    State = 0,

    /// <summary>
    /// Second layer above State. Exclusive state machine among Screens only:
    /// showing a new Screen exits all Windows and the current Screen,
    /// but leaves the current State untouched.
    /// </summary>
    Screen = 1,

    /// <summary>
    /// Additive layer above Screen. Multiple Windows can be open simultaneously
    /// as a LIFO stack. Showing a Window pushes it on top.
    /// </summary>
    Window = 2,

    /// <summary>
    /// Independent layer. Shown/hidden only by explicit calls; unaffected by
    /// State, Screen, or Window transitions. Suitable for persistent overlays
    /// (loading spinner, achievement toasts, etc.).
    /// </summary>
    Static = 3,
}
