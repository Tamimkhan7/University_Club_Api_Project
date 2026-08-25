using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.File;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.File
{
    public interface IFileService
    {
        Task<ApiResponse<FileResourceDto>> UploadAsync(FileUploadDto dto, int userId);
        Task<ApiResponse<PagedResultDto<FileResourceDto>>> GetAllAsync(int page, int pageSize);
        Task<ApiResponse<PagedResultDto<FileResourceDto>>> GetMyFilesAsync(int userId, int page, int pageSize);
        Task<ApiResponse<FileResourceDto>> GetByIdAsync(int id);
        Task<ApiResponse<FileDownloadResult>> DownloadAsync(int id);
        Task<ApiResponse<string>> DeleteAsync(int id, int userId);
        Task<ApiResponse<PagedResultDto<FileResourceDto>>> SearchAsync(string Keyword, int page, int pageSize);
        Task<ApiResponse<FileResourceDto>> UpdateAsync(int id, FileUploadDto dto, int UserId);
        Task<ApiResponse<FileStatsDto>> GetStatsAsync(int userId);
        Task<ApiResponse<PagedResultDto<FileResourceDto>>> GetByTypeAsync(string fileType, int page, int pageSize);

    }
}
