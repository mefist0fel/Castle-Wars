namespace CastleWars.Shared.Entities
{
    // Static. ColorR/G/B are 0–255 ints to avoid float on server.
    public class FactionEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
    }
}
