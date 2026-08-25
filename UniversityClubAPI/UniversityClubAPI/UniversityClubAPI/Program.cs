using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using UniversityClubAPI.Data;
using UniversityClubAPI.Filters;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.MiddleWares;
using UniversityClubAPI.Services;
using UniversityClubAPI.Services.AI;
using UniversityClubAPI.Services.Auth;
using UniversityClubAPI.Services.BadgeService;
using UniversityClubAPI.Services.ClubPrivacyService;
using UniversityClubAPI.Services.ClubService;
using UniversityClubAPI.Services.CommentService;
using UniversityClubAPI.Services.Dashboard;
using UniversityClubAPI.Services.Email;
using UniversityClubAPI.Services.EventService;
using UniversityClubAPI.Services.FeedService;
using UniversityClubAPI.Services.File;
using UniversityClubAPI.Services.FileService;
using UniversityClubAPI.Services.FollowService;
using UniversityClubAPI.Services.GroupService;
using UniversityClubAPI.Services.LeaderboardService;
using UniversityClubAPI.Services.LiveEventService;
using UniversityClubAPI.Services.MessageService;
using UniversityClubAPI.Services.NotificationService;
using UniversityClubAPI.Services.PollService;
using UniversityClubAPI.Services.PostService;
using UniversityClubAPI.Services.PresenceService;
using UniversityClubAPI.Services.ReactionService;
using UniversityClubAPI.Services.RecommendationService;
using UniversityClubAPI.Services.RecruitmentService;
using UniversityClubAPI.Services.SearchService;
using UniversityClubAPI.Services.StoryService;
using UniversityClubAPI.Services.UserService;
using UniversityClubAPI.Services.VoiceMessageService;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://yourfrontenddomain.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<ILiveEventService, LiveEventService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IClubPrivacyService, ClubPrivacyService>();
builder.Services.AddScoped<IPresenceService, PresenceService>();
builder.Services.AddScoped<IVoiceMessageService, VoiceMessageService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();


builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/chat") ||
                 path.StartsWithSegments("/hubs/notification") ||
                 path.StartsWithSegments("/hubs/group") ||
                  path.StartsWithSegments("/hubs/live")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ModeratorOnly", policy => policy.RequireRole("Admin", "Moderator"));
});


builder.Services.AddMemoryCache();


builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("fixed", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User?.Identity?.Name
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 300,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<GroupHub>("/hubs/group");
app.MapHub<LiveEventHub>("/hubs/live");

app.Run();