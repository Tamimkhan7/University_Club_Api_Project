using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Post;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.PostService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreatePostDto dto)
        {
            var callerId = UserHelper.GetUserId(User);
            var result = await _postService.CreateAsync(callerId, dto);
            return Ok(ApiResponse<PostResponseDto>.Ok(result, "Post created successfully"));
        }


        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePostDto dto)
        {
            var callerId = UserHelper.GetUserId(User);
            var result = await _postService.UpdateAsync(callerId, id, dto);
            return Ok(ApiResponse<PostResponseDto>.Ok(result, "Post updated successfully"));
        }


        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var callerId = UserHelper.GetUserId(User);
            await _postService.DeleteAsync(callerId, id);
            return Ok(ApiResponse<object>.Ok("Post deleted successfully"));
        }


        [HttpGet("all")]
        public async Task<IActionResult> All([FromQuery] PostQueryDto query)
        {
            var callerId = UserHelper.GetUserId(User);
            var result = await _postService.GetAllAsync(callerId, query);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var callerId = UserHelper.GetUserId(User);
            var result = await _postService.GetByIdAsync(callerId, id);
            return Ok(ApiResponse<PostResponseDto>.Ok(result));
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] PaginationParamsDto pagination)
        {
            var callerId = UserHelper.GetUserId(User);
            var result = await _postService.SearchAsync(callerId, query, pagination);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpPost("save/{postId:int}")]
        public async Task<IActionResult> SavePost(int postId)
        {
            var callerId = UserHelper.GetUserId(User);
            await _postService.SavePostAsync(callerId, postId);
            return Ok(ApiResponse<object>.Ok("Post saved"));
        }


        [HttpDelete("unsave/{postId:int}")]
        public async Task<IActionResult> UnsavePost(int postId)
        {
            var callerId = UserHelper.GetUserId(User);
            await _postService.UnsavePostAsync(callerId, postId);
            return Ok(ApiResponse<object>.Ok("Post unsaved"));
        }


        [HttpGet("saved")]
        public async Task<IActionResult> SavedPosts([FromQuery] PaginationParamsDto pagination)
        {
            var callerId = UserHelper.GetUserId(User);
            var result = await _postService.GetSavedAsync(callerId, pagination);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpPost("report")]
        public async Task<IActionResult> Report([FromBody] ReportPostDto dto)
        {
            var callerId = UserHelper.GetUserId(User);
            await _postService.ReportAsync(callerId, dto);
            return Ok(ApiResponse<object>.Ok("Post reported successfully"));
        }
    }
}
