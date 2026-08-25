using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.DTOs.Common;

namespace UniversityClubAPI.Helpers
{
    public static class PaginationHelper
    {
        public static Task<PagedResultDto<T>> ToPagedResultAsync<T>(IQueryable<T> query, PaginationParamsDto pagination)
            => ToPagedResultAsync(query, pagination.Page, pagination.PageSize);

        public static async Task<PagedResultDto<T>> ToPagedResultAsync<T>(IQueryable<T> query, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<T>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };
        }
    }
}
