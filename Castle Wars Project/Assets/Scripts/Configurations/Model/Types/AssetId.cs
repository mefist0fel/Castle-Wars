using Configurations.Model.Types;

namespace Configurations.Model
{
    [System.Serializable]
    public class AssetId
    {
        public readonly string guid;

        public readonly static SpriteId Invalid = new(null);
        public bool IsValid => !string.IsNullOrEmpty(guid);

        public AssetId(string guid = null) => this.guid = guid;
        public AssetId() : this(null) { }
    }

    [System.Serializable]
    public class AssetId<TInterface> : AssetId
    {
        public AssetId() : base() { }
        public AssetId(string guid = null) : base(guid) { }

        public static implicit operator AssetId<TInterface>(string guid) => new(guid);
    }
}