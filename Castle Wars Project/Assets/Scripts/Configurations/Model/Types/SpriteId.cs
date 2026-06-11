namespace Configurations.Model.Types
{
    [System.Serializable]
    public readonly struct SpriteId : System.IEquatable<SpriteId>
    {
        public readonly string id;

        public readonly static SpriteId Invalid = new(null);

        public SpriteId(string id = null)
        {
            this.id = id;
        }

        public readonly bool IsValid =>
            !string.IsNullOrEmpty(id);

        public readonly override string ToString() =>
            id ?? string.Empty;

        public readonly bool Equals(SpriteId other) => id == other.id;
        public readonly override bool Equals(object obj) => obj is SpriteId other && Equals(other);
        public readonly override int GetHashCode() => id != null ? id.GetHashCode() : 0;

        public static implicit operator string(SpriteId id) => id.id;
        public static explicit operator SpriteId(string id) => new (id);
    }
}