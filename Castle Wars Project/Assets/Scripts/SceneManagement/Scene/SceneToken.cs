/// <summary>
/// Base token — type name is used as the scene ID by default.
/// Derive a concrete class: public sealed class MyToken : SceneToken { }
/// then call sceneContext.Show(new MyToken()).
/// </summary>
public abstract class SceneToken
{
    /// <summary>
    /// Scene identifier. Matched against <see cref="SceneEntry.TokenId"/> in SceneContext.
    /// Default implementation returns the concrete type name.
    /// Override in <see cref="NamedSceneToken"/> for multi-instance scenes.
    /// </summary>
    public virtual string SceneId => GetType().Name;
}

/// <summary>
/// Token that carries typed payload data to the target scene.
/// The scene receives it via <see cref="BaseScene{TToken}.OnEnter"/>.
/// </summary>
public abstract class SceneToken<TData> : SceneToken
{
    public TData Data { get; }
    protected SceneToken(TData data) => Data = data;
}

/// <summary>
/// Token with a custom string ID instead of the type name.
/// Use when one scene class handles multiple named instances
/// (e.g., <c>new LevelToken("level_01")</c>).
/// </summary>
public abstract class NamedSceneToken : SceneToken
{
    private readonly string _id;
    public override string SceneId => _id;
    protected NamedSceneToken(string id) => _id = id;
}

/// <summary>Named token that also carries typed payload.</summary>
public abstract class NamedSceneToken<TData> : SceneToken<TData>
{
    private readonly string _id;
    public override string SceneId => _id;
    protected NamedSceneToken(string id, TData data) : base(data) => _id = id;
}
