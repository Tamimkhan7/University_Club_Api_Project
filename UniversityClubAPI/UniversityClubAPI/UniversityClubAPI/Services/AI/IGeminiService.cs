namespace UniversityClubAPI.Services.AI
{
    public interface IGeminiService
    {
        Task<string?> GenerateTextAsync(string prompt);
    }
}
