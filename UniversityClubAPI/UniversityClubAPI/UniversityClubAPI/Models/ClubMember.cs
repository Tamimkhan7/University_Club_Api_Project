using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class ClubMember
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ClubId { get; set; }
        public Club? Club { get; set; }


        public String Role { get; set; } = "member";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; internal set; }
    }
}
