namespace UniversityClubAPI.Models
{
    public class FileResource
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? OriginalName { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public int? UploadedBy { get; set; }
        public User? User { get; set; }

        public long Size { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? ClubId { get; set; }
        public Club? Club { get; set; }
    }
}
