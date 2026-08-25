using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public int CreatedBy { get; set; }
        public User? Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<GroupMember>? Members { get; set; } = new List<GroupMember>();
        public ICollection<GroupMessage>? Messages { get; set; } = new List<GroupMessage>();
    }
}