using UniversityClubAPI.DTOs.Recommendation;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.RecommendationService
{
    public interface IRecommendationService
    {
        Task<ApiResponse<List<ClubRecommendationDto>>> GetRecommendedClubsAsync(int userId, int count = 10);
        Task<ApiResponse<List<EventRecommendationDto>>> GetRecommendedEventsAsync(int userId, int count = 10);
        Task<ApiResponse<SmartDigestResultDto>> RunSmartDigestAsync(int userId);

        Task<ApiResponse<List<PersonRecommendationDto>>> GetRecommendedPeopleAsync(int userId, int count = 10);
        Task<ApiResponse<bool>> DismissClubRecommendationAsync(int userId, int clubId);
    }
}