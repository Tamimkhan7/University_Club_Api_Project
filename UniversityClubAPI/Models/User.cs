using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class User
    {

        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Bio { get; set; }
        public string ProfileImage { get; set; }

        public string Department { get; set; }
        public string Batch { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
