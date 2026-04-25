using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class Club
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //public ICollection<ClubMember>? Members { get; set; } = new List<ClubMember>();
        //public ICollection<Post>? Posts { get; set; } = new List<Post>();
    }
}
