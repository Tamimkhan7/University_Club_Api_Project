using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Search;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.SearchService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/search")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }


        [HttpGet("global")]
        public async Task<IActionResult> GlobalSearch([FromQuery] string query, [FromQuery] int limitPerType = 5)
        {
            var result = await _searchService.GlobalSearchAsync(UserHelper.GetUserId(User), query, limitPerType);
            return Ok(result);
        }


        [HttpGet("advanced")]
        public async Task<IActionResult> AdvancedSearch(
            [FromQuery] AdvancedSearchDto dto, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _searchService.AdvancedSearchAsync(UserHelper.GetUserId(User), dto, pagination);
            return Ok(result);
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string query, [FromQuery] int count = 8)
        {
            var result = await _searchService.GetSuggestionsAsync(UserHelper.GetUserId(User), query, count);
            return Ok(result);
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending([FromQuery] int days = 7, [FromQuery] int count = 10)
        {
            var result = await _searchService.GetTrendingSearchesAsync(days, count);
            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentSearches([FromQuery] int count = 10)
        {
            var result = await _searchService.GetRecentSearchesAsync(UserHelper.GetUserId(User), count);
            return Ok(result);
        }

        [HttpDelete("recent/{historyId:int}")]
        public async Task<IActionResult> DeleteRecentSearch(int historyId)
        {
            var result = await _searchService.DeleteRecentSearchAsync(UserHelper.GetUserId(User), historyId);
            return Ok(result);
        }

        [HttpDelete("recent")]
        public async Task<IActionResult> ClearRecentSearches()
        {
            var result = await _searchService.ClearRecentSearchesAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }
    }
}