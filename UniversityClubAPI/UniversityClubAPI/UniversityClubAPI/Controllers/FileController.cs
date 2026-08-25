using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.File;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.File;
namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }
        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            var result = await _fileService.UploadAsync(dto, UserHelper.GetUserId(User));
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _fileService.GetAllAsync(pagination.Page, pagination.PageSize);
            return Ok(result);
        }
        [HttpGet("my")]
        public async Task<IActionResult> MyFiles([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _fileService.GetMyFilesAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _fileService.GetByIdAsync(id);
            return Ok(result);
        }
        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _fileService.DownloadAsync(id);
            return File(result.Data!.FileBytes, result.Data.ContentType, result.Data.FileName);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _fileService.DeleteAsync(id, UserHelper.GetUserId(User));
            return Ok(result);
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string keyword,
            [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _fileService.SearchAsync(keyword, pagination.Page, pagination.PageSize);
            return Ok(result);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFile(int id, [FromForm] FileUploadDto dto)
        {
            var result = await _fileService.UpdateAsync(id, dto, UserHelper.GetUserId(User));
            return Ok(result);
        }
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _fileService.GetStatsAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }
        [HttpGet("type/{*fileType}")]
        public async Task<IActionResult> GetByType(string fileType, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _fileService.GetByTypeAsync(fileType, pagination.Page, pagination.PageSize);
            return Ok(result);
        }
    }
}