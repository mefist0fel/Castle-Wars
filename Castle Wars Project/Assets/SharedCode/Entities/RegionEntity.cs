namespace CastleWars.Shared.Entities
{
    // Static grid cell. OwnerId = FactionEntity.Id, 0 = neutral.
    public class RegionEntity : BaseEntity
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public ulong OwnerId { get; set; }
        public bool IsUnderSiege { get; set; }
    }
}
