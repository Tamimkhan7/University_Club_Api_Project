using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Search;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.SearchService
{
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchService> _logger;

        private const int MaxLimitPerType = 20;
        private const int MaxSuggestionCount = 20;
        private const int MaxTrendingCount = 50;
        private const int MaxTrendingDays = 90;

        public SearchService(AppDbContext context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }


        private async Task SaveHistoryAsync(int userId, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;

            var trimmed = query.Trim();
            if (trimmed.Length > 200) trimmed = trimmed.Substring(0, 200);

            var existing = await _context.SearchHistories
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Query == trimmed);

            if (existing != null)
            {
                existing.SearchedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            _context.SearchHistories.Add(new SearchHistory { UserId = userId, Query = trimmed });

            var count = await _context.SearchHistories.CountAsync(x => x.UserId == userId);
            if (count >= 20)
            {
                var oldest = await _context.SearchHistories
                    .Where(x => x.UserId == userId)
                    .OrderBy(x => x.SearchedAt)
                    .FirstOrDefaultAsync();
                if (oldest != null) _context.SearchHistories.Remove(oldest);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                var raced = await _context.SearchHistories
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Query == trimmed);
                if (raced != null)
                {
                    raced.SearchedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<ApiResponse<UnifiedSearchResultDto>> GlobalSearchAsync(int userId, string query, int limitPerType = 5)
        {
            if (string.IsNullOrWhiteSpace(query))
                return ApiResponse<UnifiedSearchResultDto>.Fail("Search query cannot be empty.");

            limitPerType = Math.Clamp(limitPerType, 1, MaxLimitPerType);
            var q = query.Trim();
            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && (!u.IsPrivate || u.Id == userId) &&
                    !blockedIds.Contains(u.Id) &&
                    (u.Name.Contains(q) ||
                     (u.UserName != null && u.UserName.Contains(q))))
                .OrderBy(u => u.Name)
                .Take(limitPerType)
                .Select(u => new UserSearchItemDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    UserName = u.UserName,
                    ProfileImage = u.ProfileImage,
                    Department = u.Department
                })
                .ToListAsync();

            var clubs = await _context.Clubs
                .AsNoTracking()
                .Where(c => (c.Name != null && c.Name.Contains(q)) ||
                            (c.Description != null && c.Description.Contains(q)))
                .OrderBy(c => c.Name)
                .Take(limitPerType)
                .Select(c => new ClubSearchItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    MemberCount = c.Members != null ? c.Members.Count : 0
                })
                .ToListAsync();

            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => p.Content != null && p.Content.Contains(q) &&
                    !blockedIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(limitPerType)
                .Select(p => new PostSearchItemDto
                {
                    Id = p.Id,
                    ContentSnippet = p.Content!.Length > 140 ? p.Content.Substring(0, 140) + "..." : p.Content,
                    ImageUrl = p.ImageUrl,
                    UserId = p.UserId,
                    UserName = p.User!.Name,
                    ClubId = p.ClubId,
                    ClubName = p.Club!.Name,
                    CreatedAt = p.CreatedAt,
                    ReactionCount = p.Reactions.Count
                })
                .ToListAsync();

            var events = await _context.Events
                .AsNoTracking()
                .Where(e => ((e.Title != null && e.Title.Contains(q)) ||
                            (e.Description != null && e.Description.Contains(q))) &&
                    !blockedIds.Contains(e.CreatedBy))
                .OrderBy(e => e.EventDate)
                .Take(limitPerType)
                .Select(e => new EventSearchItemDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    ClubId = e.ClubId,
                    ClubName = e.club!.Name,
                    AttendeeCount = e.Attendances.Count
                })
                .ToListAsync();


            var groups = await _context.Groups
                .AsNoTracking()
                .Where(g => !blockedIds.Contains(g.CreatedBy) && g.Name.Contains(q))
                .OrderBy(g => g.Name)
                .Take(limitPerType)
                .Select(g => new GroupSearchItemDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    MemberCount = g.Members != null ? g.Members.Count : 0,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();
            var files = await _context.FileResources
                .AsNoTracking()
                .Where(f => (f.UploadedBy == null || !blockedIds.Contains(f.UploadedBy.Value)) &&
                    ((f.FileName != null && f.FileName.Contains(q)) ||
                     (f.OriginalName != null && f.OriginalName.Contains(q))))
                .OrderByDescending(f => f.UploadedAt)
                .Take(limitPerType)
                .Select(f => new FileSearchItemDto
                {
                    Id = f.Id,
                    FileName = f.OriginalName ?? f.FileName,
                    FileType = f.FileType,
                    Size = f.Size,
                    ClubId = f.ClubId,
                    ClubName = f.Club != null ? f.Club.Name : null,
                    UploaderName = f.User != null ? f.User.Name : null,
                    UploadedAt = f.UploadedAt
                })
                .ToListAsync();

            await SaveHistoryAsync(userId, query);

            var result = new UnifiedSearchResultDto
            {
                Query = query,
                Users = users,
                Clubs = clubs,
                Posts = posts,
                Events = events,
                Groups = groups,
                Files = files,
                TotalResults = users.Count + clubs.Count + posts.Count + events.Count + groups.Count + files.Count
            };

            return ApiResponse<UnifiedSearchResultDto>.Ok(result);
        }

        public async Task<ApiResponse<AdvancedSearchResultDto>> AdvancedSearchAsync(
            int userId, AdvancedSearchDto dto, PaginationParamsDto pagination)
        {
            var q = dto.Query?.Trim() ?? string.Empty;
            var result = new AdvancedSearchResultDto { Type = dto.Type };
            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            switch (dto.Type)
            {
                case SearchEntityType.Users:
                    {
                        var query = _context.Users.AsNoTracking()
                            .Where(u => !u.IsDeleted && (!u.IsPrivate || u.Id == userId) &&
                                !blockedIds.Contains(u.Id));

                        if (!string.IsNullOrWhiteSpace(q))
                            query = query.Where(u => u.Name.Contains(q) ||
                                (u.UserName != null && u.UserName.Contains(q)) ||
                                (u.Department != null && u.Department.Contains(q)));

                        query = dto.SortBy switch
                        {
                            SearchSortBy.Newest => query.OrderByDescending(u => u.CreatedAt),
                            SearchSortBy.Oldest => query.OrderBy(u => u.CreatedAt),
                            _ => query.OrderBy(u => u.Name)
                        };

                        var mapped = query.Select(u => new UserSearchItemDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            UserName = u.UserName,
                            ProfileImage = u.ProfileImage,
                            Department = u.Department
                        });

                        var paged = await PaginationHelper.ToPagedResultAsync(mapped, pagination);
                        result.Users = paged.Items;
                        result.Page = paged.Page; result.PageSize = paged.PageSize;
                        result.TotalCount = paged.TotalCount; result.TotalPages = paged.TotalPages;
                        break;
                    }

                case SearchEntityType.Clubs:
                    {
                        var query = _context.Clubs.AsNoTracking().AsQueryable();

                        if (!string.IsNullOrWhiteSpace(q))
                            query = query.Where(c => (c.Name != null && c.Name.Contains(q)) ||
                                                      (c.Description != null && c.Description.Contains(q)));

                        query = dto.SortBy switch
                        {
                            SearchSortBy.Newest => query.OrderByDescending(c => c.CreatedAt),
                            SearchSortBy.Oldest => query.OrderBy(c => c.CreatedAt),
                            SearchSortBy.Popular => query.OrderByDescending(c => c.Members!.Count),
                            _ => query.OrderBy(c => c.Name)
                        };

                        var mapped = query.Select(c => new ClubSearchItemDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            MemberCount = c.Members != null ? c.Members.Count : 0
                        });

                        var paged = await PaginationHelper.ToPagedResultAsync(mapped, pagination);
                        result.Clubs = paged.Items;
                        result.Page = paged.Page; result.PageSize = paged.PageSize;
                        result.TotalCount = paged.TotalCount; result.TotalPages = paged.TotalPages;
                        break;
                    }

                case SearchEntityType.Posts:
                    {
                        var query = _context.Posts.AsNoTracking()
                            .Where(p => !blockedIds.Contains(p.UserId));

                        if (!string.IsNullOrWhiteSpace(q))
                            query = query.Where(p => p.Content != null && p.Content.Contains(q));

                        if (dto.ClubId.HasValue)
                            query = query.Where(p => p.ClubId == dto.ClubId.Value);

                        if (dto.FromDate.HasValue)
                            query = query.Where(p => p.CreatedAt >= dto.FromDate.Value);

                        if (dto.ToDate.HasValue)
                            query = query.Where(p => p.CreatedAt <= dto.ToDate.Value);

                        query = dto.SortBy switch
                        {
                            SearchSortBy.Oldest => query.OrderBy(p => p.CreatedAt),
                            SearchSortBy.Popular => query.OrderByDescending(p => p.Reactions.Count),
                            _ => query.OrderByDescending(p => p.CreatedAt)
                        };

                        var mapped = query.Select(p => new PostSearchItemDto
                        {
                            Id = p.Id,
                            ContentSnippet = p.Content!.Length > 140 ? p.Content.Substring(0, 140) + "..." : p.Content,
                            ImageUrl = p.ImageUrl,
                            UserId = p.UserId,
                            UserName = p.User!.Name,
                            ClubId = p.ClubId,
                            ClubName = p.Club!.Name,
                            CreatedAt = p.CreatedAt,
                            ReactionCount = p.Reactions.Count
                        });

                        var paged = await PaginationHelper.ToPagedResultAsync(mapped, pagination);
                        result.Posts = paged.Items;
                        result.Page = paged.Page; result.PageSize = paged.PageSize;
                        result.TotalCount = paged.TotalCount; result.TotalPages = paged.TotalPages;
                        break;
                    }

                case SearchEntityType.Events:
                    {
                        var query = _context.Events.AsNoTracking()
                            .Where(e => !blockedIds.Contains(e.CreatedBy));

                        if (!string.IsNullOrWhiteSpace(q))
                            query = query.Where(e => (e.Title != null && e.Title.Contains(q)) ||
                                                      (e.Description != null && e.Description.Contains(q)));

                        if (dto.ClubId.HasValue)
                            query = query.Where(e => e.ClubId == dto.ClubId.Value);

                        if (dto.FromDate.HasValue)
                            query = query.Where(e => e.EventDate >= dto.FromDate.Value);

                        if (dto.ToDate.HasValue)
                            query = query.Where(e => e.EventDate <= dto.ToDate.Value);

                        query = dto.SortBy switch
                        {
                            SearchSortBy.Newest => query.OrderByDescending(e => e.EventDate),
                            SearchSortBy.Popular => query.OrderByDescending(e => e.Attendances.Count),
                            _ => query.OrderBy(e => e.EventDate)
                        };

                        var mapped = query.Select(e => new EventSearchItemDto
                        {
                            Id = e.Id,
                            Title = e.Title,
                            Description = e.Description,
                            EventDate = e.EventDate,
                            ClubId = e.ClubId,
                            ClubName = e.club!.Name,
                            AttendeeCount = e.Attendances.Count
                        });

                        var paged = await PaginationHelper.ToPagedResultAsync(mapped, pagination);
                        result.Events = paged.Items;
                        result.Page = paged.Page; result.PageSize = paged.PageSize;
                        result.TotalCount = paged.TotalCount; result.TotalPages = paged.TotalPages;
                        break;
                    }

                case SearchEntityType.Groups:
                    {
                        var query = _context.Groups.AsNoTracking()
                            .Where(g => !blockedIds.Contains(g.CreatedBy));

                        if (!string.IsNullOrWhiteSpace(q))
                            query = query.Where(g => g.Name.Contains(q));

                        query = dto.SortBy switch
                        {
                            SearchSortBy.Newest => query.OrderByDescending(g => g.CreatedAt),
                            SearchSortBy.Oldest => query.OrderBy(g => g.CreatedAt),
                            SearchSortBy.Popular => query.OrderByDescending(g => g.Members!.Count),
                            _ => query.OrderBy(g => g.Name)
                        };

                        var mapped = query.Select(g => new GroupSearchItemDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            MemberCount = g.Members != null ? g.Members.Count : 0,
                            CreatedAt = g.CreatedAt
                        });

                        var paged = await PaginationHelper.ToPagedResultAsync(mapped, pagination);
                        result.Groups = paged.Items;
                        result.Page = paged.Page; result.PageSize = paged.PageSize;
                        result.TotalCount = paged.TotalCount; result.TotalPages = paged.TotalPages;
                        break;
                    }


                case SearchEntityType.Files:
                    {
                        var query = _context.FileResources.AsNoTracking()
                            .Where(f => f.UploadedBy == null || !blockedIds.Contains(f.UploadedBy.Value));

                        if (!string.IsNullOrWhiteSpace(q))
                            query = query.Where(f =>
                                (f.FileName != null && f.FileName.Contains(q)) ||
                                (f.OriginalName != null && f.OriginalName.Contains(q)));

                        if (dto.ClubId.HasValue)
                            query = query.Where(f => f.ClubId == dto.ClubId.Value);

                        if (dto.FromDate.HasValue)
                            query = query.Where(f => f.UploadedAt >= dto.FromDate.Value);

                        if (dto.ToDate.HasValue)
                            query = query.Where(f => f.UploadedAt <= dto.ToDate.Value);

                        query = dto.SortBy switch
                        {
                            SearchSortBy.Oldest => query.OrderBy(f => f.UploadedAt),
                            _ => query.OrderByDescending(f => f.UploadedAt)
                        };

                        var mapped = query.Select(f => new FileSearchItemDto
                        {
                            Id = f.Id,
                            FileName = f.OriginalName ?? f.FileName,
                            FileType = f.FileType,
                            Size = f.Size,
                            ClubId = f.ClubId,
                            ClubName = f.Club != null ? f.Club.Name : null,
                            UploaderName = f.User != null ? f.User.Name : null,
                            UploadedAt = f.UploadedAt
                        });

                        var paged = await PaginationHelper.ToPagedResultAsync(mapped, pagination);
                        result.Files = paged.Items;
                        result.Page = paged.Page; result.PageSize = paged.PageSize;
                        result.TotalCount = paged.TotalCount; result.TotalPages = paged.TotalPages;
                        break;
                    }
            }

            if (!string.IsNullOrWhiteSpace(dto.Query))
                await SaveHistoryAsync(userId, dto.Query);

            return ApiResponse<AdvancedSearchResultDto>.Ok(result);
        }

        public async Task<ApiResponse<List<SearchSuggestionDto>>> GetSuggestionsAsync(int userId, string query, int count = 8)
        {
            if (string.IsNullOrWhiteSpace(query))
                return ApiResponse<List<SearchSuggestionDto>>.Ok(new List<SearchSuggestionDto>());

            count = Math.Clamp(count, 1, MaxSuggestionCount);
            var q = query.Trim();
            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var userSuggestions = await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.Id != userId && !blockedIds.Contains(u.Id) &&
                    !u.IsPrivate &&
                    (u.Name.StartsWith(q) || (u.UserName != null && u.UserName.StartsWith(q))))
                .OrderBy(u => u.Name)
                .Take(count)
                .Select(u => new SearchSuggestionDto { Type = SearchEntityType.Users, Id = u.Id, Label = u.Name })
                .ToListAsync();

            var clubSuggestions = await _context.Clubs
                .AsNoTracking()
                .Where(c => c.Name != null && c.Name.StartsWith(q))
                .OrderBy(c => c.Name)
                .Take(count)
                .Select(c => new SearchSuggestionDto { Type = SearchEntityType.Clubs, Id = c.Id, Label = c.Name! })
                .ToListAsync();

            var combined = userSuggestions
                .Concat(clubSuggestions)
                .OrderBy(s => s.Label)
                .Take(count)
                .ToList();

            return ApiResponse<List<SearchSuggestionDto>>.Ok(combined);
        }

        public async Task<ApiResponse<List<TrendingSearchDto>>> GetTrendingSearchesAsync(int days = 7, int count = 10)
        {
            days = Math.Clamp(days, 1, MaxTrendingDays);
            count = Math.Clamp(count, 1, MaxTrendingCount);
            var since = DateTime.UtcNow.AddDays(-days);

            var trending = await _context.SearchHistories
                .AsNoTracking()
                .Where(x => x.SearchedAt >= since)
                .GroupBy(x => x.Query)
                .Select(g => new TrendingSearchDto { Query = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToListAsync();

            return ApiResponse<List<TrendingSearchDto>>.Ok(trending);
        }

        public async Task<ApiResponse<List<RecentSearchDto>>> GetRecentSearchesAsync(int userId, int count = 10)
        {
            count = Math.Clamp(count, 1, 50);

            var history = await _context.SearchHistories
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.SearchedAt)
                .Take(count)
                .Select(x => new RecentSearchDto { Id = x.Id, Query = x.Query, SearchedAt = x.SearchedAt })
                .ToListAsync();

            return ApiResponse<List<RecentSearchDto>>.Ok(history);
        }

        public async Task<ApiResponse<string>> DeleteRecentSearchAsync(int userId, int historyId)
        {
            var entry = await _context.SearchHistories
                .FirstOrDefaultAsync(x => x.Id == historyId && x.UserId == userId);

            if (entry == null)
                return ApiResponse<string>.Fail("Search history entry not found.");

            _context.SearchHistories.Remove(entry);
            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Removed from recent searches.");
        }

        public async Task<ApiResponse<string>> ClearRecentSearchesAsync(int userId)
        {
            var entries = await _context.SearchHistories.Where(x => x.UserId == userId).ToListAsync();
            _context.SearchHistories.RemoveRange(entries);
            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Search history cleared.");
        }
    }
}