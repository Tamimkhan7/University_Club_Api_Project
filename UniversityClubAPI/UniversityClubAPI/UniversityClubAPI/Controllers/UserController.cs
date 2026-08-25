using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.User;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.UserService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }



        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
            => Ok(await _userService.GetMyProfileAsync(UserHelper.GetUserId(User)));


        [HttpGet("profile/{id:int}")]
        public async Task<IActionResult> GetProfileById(int id)
            => Ok(await _userService.GetProfileByIdAsync(UserHelper.GetUserId(User), id));


        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] UpdateUserDto dto)
            => Ok(await _userService.UpdateProfileAsync(UserHelper.GetUserId(User), dto));


        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
            => Ok(await _userService.ChangePasswordAsync(UserHelper.GetUserId(User), dto));



        [HttpPut("deactivate")]
        public async Task<IActionResult> Deactivate()
            => Ok(await _userService.DeactivateAsync(UserHelper.GetUserId(User)));


        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
            => Ok(await _userService.SoftDeleteAsync(UserHelper.GetUserId(User)));


        [HttpPut("privacy")]
        public async Task<IActionResult> SetPrivacy([FromQuery] bool isPrivate)
            => Ok(await _userService.SetPrivacyAsync(UserHelper.GetUserId(User), isPrivate));

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] UserQueryDto filter)
            => Ok(await _userService.GetAllAsync(UserHelper.GetUserId(User), filter));

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] UserQueryDto filter)
            => Ok(await _userService.SearchAsync(UserHelper.GetUserId(User), filter));


        [HttpGet("stats/{id:int}")]
        public async Task<IActionResult> Stats(int id)
            => Ok(await _userService.GetStatsAsync(id));


        [HttpGet("posts/count")]
        public async Task<IActionResult> MyPostsCount()
        {
            var callerId = UserHelper.GetUserId(User);
            var stats = await _userService.GetStatsAsync(callerId);
            return Ok(ApiResponse<object>.Ok(new { count = stats.Data!.Posts }));
        }


        [HttpPost("follow/{id:int}")]
        public async Task<IActionResult> Follow(int id)
            => Ok(await _userService.FollowAsync(UserHelper.GetUserId(User), id));


        [HttpDelete("follow/{id:int}")]
        public async Task<IActionResult> Unfollow(int id)
            => Ok(await _userService.UnfollowAsync(UserHelper.GetUserId(User), id));


        [HttpGet("followers/{id:int}")]
        public async Task<IActionResult> Followers(int id, [FromQuery] PaginationParamsDto pagination)
            => Ok(await _userService.GetFollowersAsync(UserHelper.GetUserId(User), id, pagination));


        [HttpGet("following/{id:int}")]
        public async Task<IActionResult> Following(int id, [FromQuery] PaginationParamsDto pagination)
            => Ok(await _userService.GetFollowingAsync(UserHelper.GetUserId(User), id, pagination));


        [HttpGet("mutual/{id:int}")]
        public async Task<IActionResult> MutualFollows(int id, [FromQuery] PaginationParamsDto pagination)
            => Ok(await _userService.GetMutualFollowsAsync(UserHelper.GetUserId(User), id, pagination));


        [HttpPost("block/{id:int}")]
        public async Task<IActionResult> Block(int id)
            => Ok(await _userService.BlockAsync(UserHelper.GetUserId(User), id));


        [HttpDelete("unblock/{id:int}")]
        public async Task<IActionResult> Unblock(int id)
            => Ok(await _userService.UnblockAsync(UserHelper.GetUserId(User), id));


        [HttpGet("blocked")]
        public async Task<IActionResult> GetBlocked([FromQuery] PaginationParamsDto pagination)
            => Ok(await _userService.GetBlockedAsync(UserHelper.GetUserId(User), pagination));


        [HttpPost("profile-views/{id:int}")]
        public async Task<IActionResult> RecordProfileView(int id)
            => Ok(await _userService.RecordProfileViewAsync(UserHelper.GetUserId(User), id));
    }
}