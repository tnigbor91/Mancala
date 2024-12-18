

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SS.Mancala.BL.Models
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public int Score { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }

        public Player(string username, int score = 0)
        {
            Id = Guid.NewGuid();
            Username = username;
            Score = score;
        }

        //public User(Guid userId, string username, int score = 0)
        //{
        //    Id = userId;
        //    Username = username;
        //    Score = score;
        //}

        public Player() { Id = Guid.NewGuid(); }

        //public void SetId(Guid id)
        //{
        //    if (Id == Guid.Empty)
        //    {
        //        Id = id;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("Id has already been set.");
        //    }
        //}

        //public void UpdateScore(int points)
        //{
        //    Score += points;
        //}

        //public override string ToString()
        //{
        //    return $"Player(UserId: {Id}, Username: {Username}, Score: {Score})";
        //}
    }
}