namespace CastleWars.Shared.Entities
{
    // Static. FactionId links to FactionEntity.
    public class PlayerEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ulong FactionId { get; set; }
        public int Gold { get; set; }
    }
}
