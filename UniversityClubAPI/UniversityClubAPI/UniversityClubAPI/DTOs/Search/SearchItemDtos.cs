namespace UniversityClubAPI.DTOs.Search
{
    public class UserSearchItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Department { get; set; }
    }

    public class ClubSearchItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int MemberCount { get; set; }
    }

    public class PostSearchItemDto
    {
        public int Id { get; set; }
        public string? ContentSnippet { get; set; }
        public string? ImageUrl { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int ClubId { get; set; }
        public string? ClubName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReactionCount { get; set; }
    }

    public class EventSearchItemDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int ClubId { get; set; }
        public string? ClubName { get; set; }
        public int AttendeeCount { get; set; }
    }


    public class GroupSearchItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FileSearchItemDto
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public long Size { get; set; }
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? UploaderName { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}