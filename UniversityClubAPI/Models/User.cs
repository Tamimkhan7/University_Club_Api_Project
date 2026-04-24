using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "User";

        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // NAVIGATION FIXED
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}