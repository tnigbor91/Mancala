using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Mancala.BL.Models
{
  public class User
    {
        public Guid Id { get; set; }
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [Required]
        [DisplayName("User Id")]
        public string UserId { get; set; }
        [Required]
        [DisplayName("Password")]
        public string Password { get; set; }

        public string FullName { get { return FirstName + " " + LastName; } }
    }
}
