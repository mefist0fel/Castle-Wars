namespace CastleWars.Shared.Entities
{
    public abstract class BaseEntity
    {
        public ulong Id { get; set; }
        public uint Version { get; set; }
    }
}
