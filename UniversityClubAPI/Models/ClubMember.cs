using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class ClubMember
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ClubId { get; set; }
        public String Role { get; set; } = "Member"; // Default role is "Member"
    }
}
