namespace UniversityClubAPI.DTOs.File
{
    public class FileResourceDto
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? OriginalName { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public long Size { get; set; }
        public int? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
