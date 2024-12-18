namespace SS.Mancala.BL.Models
{
    public class Move
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public int SourcePit { get; set; }
        public int StonesMoved { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool IsExtraTurn { get; set; }
        public int MoveNo { get; set; }


        public Move(Guid gameId, Guid playerId, int sourcePit, int stonesMoved, int moveNo)
        {
            Id = Guid.NewGuid();
            GameId = gameId;
            PlayerId = playerId;
            SourcePit = sourcePit;
            StonesMoved = stonesMoved;
            TimeStamp = DateTime.Now;
            IsExtraTurn = false;
            MoveNo = moveNo;
        }

        public Move()
        {
        }
    }
}
