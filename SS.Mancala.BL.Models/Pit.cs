namespace SS.Mancala.BL.Models
{
    public class Pit
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public int Stones { get; set; }
        public bool IsMancala { get; set; }
        public int PitPosition { get; set; }

        public Pit(Guid playerId, int pitPosition, bool isMancala, int initialStones)
        {
            Id = Guid.NewGuid();
            PlayerId = playerId;
            PitPosition = pitPosition;
            IsMancala = isMancala;
            Stones = initialStones;
        }

        // Default constructor
        public Pit()
        {
            Id = Guid.NewGuid();
            Stones = 0;
            PitPosition = 0;
            IsMancala = false;
        }

        public void AddStones(int count)
        {
            Stones += count;
        }

        public void RemoveStones(int count)
        {
            if (count > Stones)
                throw new InvalidOperationException("Cannot remove more stones than available in the pit.");
            Stones -= count;
        }
    }
}
