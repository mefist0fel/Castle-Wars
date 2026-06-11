using UnityEngine.SceneManagement;

internal sealed class SceneHandle
{
    public readonly string     TokenId;
    public readonly string     SceneName;
    public readonly SceneType Layer;
    public readonly bool       Cached;

    public object Token  { get; set; }
    public Scene UnityScene { get; set; }
    public IScene Controller  { get; set; }
    public bool   IsLoaded    { get; set; }

    public SceneHandle(SceneEntry entry)
    {
        TokenId   = entry.TokenId;
        SceneName = entry.SceneName;
        Layer     = entry.Layer;
        Cached    = entry.Cached;
    }
}
