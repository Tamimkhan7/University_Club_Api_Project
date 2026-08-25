using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Comment;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.CommentService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        private int GetUserId() => UserHelper.GetUserId(User);

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
            => Ok(await _commentService.CreateAsync(GetUserId(), dto));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCommentDto dto)
            => Ok(await _commentService.UpdateAsync(GetUserId(), id, dto));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await _commentService.DeleteAsync(GetUserId(), id));

        [HttpGet("post/{postId:int}")]
        public async Task<IActionResult> GetComments(int postId, [FromQuery] PaginationParamsDto pagination)
            => Ok(await _commentService.GetPostCommentsAsync(GetUserId(), postId, pagination.Page, pagination.PageSize));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _commentService.GetCommentByIdAsync(GetUserId(), id));

        [HttpGet("{commentId:int}/replies")]
        public async Task<IActionResult> Replies(int commentId)
            => Ok(await _commentService.GetRepliesAsync(GetUserId(), commentId));

        [HttpPost("{commentId:int}/like")]
        public async Task<IActionResult> ToggleLike(int commentId)
            => Ok(await _commentService.ToggleLikeAsync(GetUserId(), commentId));

        [HttpGet("{commentId:int}/likes")]
        public async Task<IActionResult> LikeCount(int commentId)
        {
            var result = await _commentService.GetLikeCountAsync(commentId);
            return Ok(ApiResponse<object>.Ok(new
            {
                commentId,
                likeCount = result.Data
            }));
        }
    }
}