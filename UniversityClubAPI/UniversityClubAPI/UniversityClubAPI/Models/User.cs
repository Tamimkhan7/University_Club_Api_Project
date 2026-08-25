namespace UniversityClubAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public string? CoverPhoto { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }
        public string Role { get; set; } = "Member";
        public bool IsPrivate { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool IsOnline { get; set; } = false;
        public DateTime? LastSeenAt { get; set; }
        public int ProfileViews { get; set; } = 0;


        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }


        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }


        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}