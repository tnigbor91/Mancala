using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.BL.Models
{
    public class Game
    {
        private string _displayName;
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    return $"Game {DateCreated:MM/dd/yyyy HH:mm}";
                }
                return _displayName;
            }
            set { _displayName = value; }
        }

        public Guid Id { get; set; }
        public Guid Player1Id { get; set; }
        public Guid Player2Id { get; set; }
        public bool IsGameOver { get; set; }
        public string Status { get; set; }
        public Guid CurrentTurn { get; set; }
        public DateTime DateCreated { get; set; }
        public List<Pit> Pits { get; set; }
        public List<Move> Moves { get; set; }
        public Game()
        {
            Id = Guid.NewGuid();
            Pits = new List<Pit>();
            Moves = new List<Move>();
        }

        public Game(Guid player1Id, Guid player2Id)
        {
            Id = Guid.NewGuid();
            Player1Id = player1Id;
            Player2Id = player2Id;
            DateCreated = DateTime.Now;
            IsGameOver = false;
            CurrentTurn = player1Id;
        }

        public Game(Guid gameId, Guid player1Id, Guid player2Id) : this()
        {
            Id = gameId;
            InitializeBoard(player1Id, player2Id);
        }


        public void InitializeBoard(Guid player1Id, Guid player2Id)
        {
            Pits.Clear();

            // Player 1's pits (index 0 to 5)
            for (int i = 0; i < 6; i++)
            {
                Pits.Add(new Pit
                {
                    Id = Guid.NewGuid(),
                    PitPosition = i,
                    Stones = 4,
                    PlayerId = player1Id, // This links the pit to the player
                    IsMancala = false
                });
            }

            // Player 1's Mancala (index 6)
            Pits.Add(new Pit
            {
                Id = Guid.NewGuid(),
                PitPosition = 6,
                Stones = 0,
                PlayerId = player1Id, // Player's Mancala
                IsMancala = true
            });

            // Player 2's pits (index 7 to 12)
            for (int i = 7; i < 13; i++)
            {
                Pits.Add(new Pit
                {
                    Id = Guid.NewGuid(),
                    PitPosition = i,
                    Stones = 4,
                    PlayerId = player2Id, // Player's pits
                    IsMancala = false
                });
            }

            // Player 2's Mancala (index 13)
            Pits.Add(new Pit
            {
                Id = Guid.NewGuid(),
                PitPosition = 13,
                Stones = 0,
                PlayerId = player2Id, // Player's Mancala
                IsMancala = true
            });
        }
    }
}



