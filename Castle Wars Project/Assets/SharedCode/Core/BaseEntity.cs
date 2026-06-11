#nullable enable
namespace CastleWars.Shared.Core
{
    public abstract class BaseEntity
    {
        public ulong Id { get; internal set; }
        public uint Version { get; internal set; }
    }
}
