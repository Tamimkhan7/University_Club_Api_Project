namespace UniversityClubAPI.DTOs.File
{
    public class FileStatsDto
    {
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public Dictionary<string, int> FileCountByType { get; set; } = new();
        public FileResourceDto? LastUploaded { get; set; }
    }
}
