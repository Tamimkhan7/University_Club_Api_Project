using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.File;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.File;

namespace UniversityClubAPI.Services.FileService
{
    public class FileService : IFileService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".ppt", ".pptx" };

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        public FileService(AppDbContext context, IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Checks size, extension, and content-type against the allowed lists. Throws ArgumentException if invalid.
        /// </summary>
        private void ValidateFile(IFormFile file)
        {
            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("File size must be less than 10 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Extension '{extension}' is not allowed.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException($"Content-type '{file.ContentType}' is not allowed.");
        }

        /// <summary>
        /// Writes the file to the upload folder under a new GUID name and returns that file name.
        /// Call ValidateFile first.
        /// </summary>
        private async Task<string> SaveFileToDiskAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uploadFolder = GetUploadFolder();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            return uniqueFileName;
        }

        public async Task<ApiResponse<FileResourceDto>> UploadAsync(FileUploadDto dto, int userId)
        {
            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("File is required.");

            ValidateFile(dto.File);

            var uniqueFileName = await SaveFileToDiskAsync(dto.File);

            var fileResource = new FileResource
            {
                FileName = dto.File.FileName,
                OriginalName = dto.File.FileName,
                FileUrl = "/uploads/" + uniqueFileName,
                FileType = dto.File.ContentType,
                UploadedBy = userId,
                Size = dto.File.Length,
                IsDeleted = false
            };

            _context.FileResources.Add(fileResource);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} uploaded file {FileId}", userId, fileResource.Id);

            return ApiResponse<FileResourceDto>.Ok(MapToDto(fileResource), "File uploaded successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<FileResourceDto>>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.FileResources
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.UploadedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);

            var result = new PagedResultDto<FileResourceDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };

            return ApiResponse<PagedResultDto<FileResourceDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<FileResourceDto>>> GetMyFilesAsync(int userId, int page, int pageSize)
        {
            var query = _context.FileResources
                .AsNoTracking()
                .Where(x => x.UploadedBy == userId && !x.IsDeleted)
                .OrderByDescending(x => x.UploadedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);

            var result = new PagedResultDto<FileResourceDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };

            return ApiResponse<PagedResultDto<FileResourceDto>>.Ok(result);
        }

        public async Task<ApiResponse<FileResourceDto>> GetByIdAsync(int id)
        {
            var file = await _context.FileResources
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (file == null)
                throw new KeyNotFoundException("File not found.");

            return ApiResponse<FileResourceDto>.Ok(MapToDto(file));
        }

        public async Task<ApiResponse<FileDownloadResult>> DownloadAsync(int id)
        {
            var file = await _context.FileResources
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (file == null)
                throw new KeyNotFoundException("File not found.");

            var relativePath = file.FileUrl!.TrimStart('/');
            var filePath = Path.Combine(GetWebRoot(), relativePath);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError("File record {FileId} exists but physical file is missing at {Path}", id, filePath);
                throw new InvalidOperationException("File could not be retrieved. Please try again later.");
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return ApiResponse<FileDownloadResult>.Ok(new FileDownloadResult
            {
                FileBytes = bytes,
                ContentType = file.FileType!,
                FileName = file.FileName!
            });
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id, int userId)
        {
            var file = await _context.FileResources
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (file == null)
                throw new KeyNotFoundException("File not found.");

            if (file.UploadedBy != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this file.");

            var filePath = Path.Combine(GetWebRoot(), file.FileUrl!.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            file.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted file {FileId}", userId, id);

            return ApiResponse<string>.Ok("File deleted successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<FileResourceDto>>> SearchAsync(string keyword, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException("Keyword is required.");

            var query = _context.FileResources
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    (x.FileName!.Contains(keyword) ||
                     x.FileType!.Contains(keyword) ||
                     x.OriginalName!.Contains(keyword)))
                .OrderByDescending(x => x.UploadedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);

            var result = new PagedResultDto<FileResourceDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };

            return ApiResponse<PagedResultDto<FileResourceDto>>.Ok(result);
        }

        public async Task<ApiResponse<FileResourceDto>> UpdateAsync(int id, FileUploadDto dto, int userId)
        {
            var file = await _context.FileResources
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (file == null)
                throw new KeyNotFoundException("File not found.");

            if (file.UploadedBy != userId)
                throw new UnauthorizedAccessException("You are not allowed to update this file.");

            if (dto.File != null && dto.File.Length > 0)
            {
                ValidateFile(dto.File);

                var oldPath = Path.Combine(GetWebRoot(), file.FileUrl!.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                var uniqueFileName = await SaveFileToDiskAsync(dto.File);

                file.FileUrl = "/uploads/" + uniqueFileName;
                file.FileType = dto.File.ContentType;
                file.Size = dto.File.Length;
                file.OriginalName = dto.File.FileName;
                file.FileName = dto.File.FileName;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated file {FileId}", userId, id);

            return ApiResponse<FileResourceDto>.Ok(MapToDto(file), "File updated successfully.");
        }

        public async Task<ApiResponse<FileStatsDto>> GetStatsAsync(int userId)
        {
            var files = await _context.FileResources
                .AsNoTracking()
                .Where(x => x.UploadedBy == userId && !x.IsDeleted)
                .ToListAsync();

            var stats = new FileStatsDto
            {
                TotalFiles = files.Count,
                TotalSize = files.Sum(x => x.Size),
                FileCountByType = files
                    .GroupBy(x => x.FileType ?? "unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                LastUploaded = files
                    .OrderByDescending(x => x.UploadedAt)
                    .Select(MapToDto)
                    .FirstOrDefault()
            };

            return ApiResponse<FileStatsDto>.Ok(stats);
        }

        public async Task<ApiResponse<PagedResultDto<FileResourceDto>>> GetByTypeAsync(string fileType, int page, int pageSize)
        {
            var query = _context.FileResources
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.FileType!.Contains(fileType))
                .OrderByDescending(x => x.UploadedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);

            var result = new PagedResultDto<FileResourceDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };

            return ApiResponse<PagedResultDto<FileResourceDto>>.Ok(result);
        }

        private string GetWebRoot()
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(webRoot))
                    Directory.CreateDirectory(webRoot);
            }

            return webRoot;
        }

        private string GetUploadFolder()
        {
            var folder = Path.Combine(GetWebRoot(), "uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return folder;
        }

        private static FileResourceDto MapToDto(FileResource x) => new()
        {
            Id = x.Id,
            FileName = x.FileName,
            OriginalName = x.OriginalName,
            FileUrl = x.FileUrl,
            FileType = x.FileType,
            Size = x.Size,
            UploadedBy = x.UploadedBy,
            UploadedAt = x.UploadedAt
        };
    }
}