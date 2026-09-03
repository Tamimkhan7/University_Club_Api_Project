# University Club API

A full-featured **university club management and social networking platform backend**, built with **ASP.NET Core 9 (Web API)**. Beyond club management, it includes a mini social media layer — posts, feeds, stories, chat, live events, badges, leaderboards, and AI-driven recommendations.

---

## Overview

This API powers a platform where students can:

- Create and manage university clubs
- Post content, host events, and run polls within clubs
- Follow other users and get a personalized feed
- Send direct messages and group messages (text and voice)
- Host live events with real-time chat
- Share 24-hour stories
- Earn badges and compete on leaderboards based on contribution
- Get AI-powered personalized recommendations and smart digests
- Apply to clubs through a recruitment/application system

The project follows a clean, layered architecture: Controller → Service (Interface + Implementation) → DTO → EF Core Model.

---

## Tech Stack

- **.NET 9 / ASP.NET Core Web API** — core framework
- **Entity Framework Core 9 (SQL Server)** — ORM and database migrations
- **JWT Bearer Authentication** — access and refresh token based auth
- **BCrypt.Net-Next** — password hashing
- **SignalR** — real-time chat, notifications, and live events
- **CloudinaryDotNet** — image/video/voice upload and CDN hosting
- **NEST (Elasticsearch client)** — advanced search
- **Swashbuckle (Swagger)** — API documentation
- **SMTP (System.Net.Mail)** — email verification and password reset emails
- **Google Gemini API** — AI-driven insights and recommendations
- **Custom rate limiting middleware** — 300 requests per minute per user/IP
- **Custom exception middleware** — global error handling

---

## Project Structure

```
UniversityClubAPI/
├── Controllers/         25 API controllers (Auth, Club, Post, Event, ...)
├── Services/             Business logic (interface + implementation per module)
│   ├── Auth/  Club/  Post/  Event/  Feed/  Group/  Message/
│   ├── Notification/  Badge/  Leaderboard/  Recommendation/
│   ├── LiveEvent/  Recruitment/  Search/  Story/  VoiceMessage/
│   ├── Presence/  ClubPrivacy/  Dashboard/  Email/  AI (Gemini)/
│   └── ImageService.cs (Cloudinary)
├── Models/               EF Core entity/domain models (Club, Post, User, ...)
├── DTOs/                 Request and response data transfer objects
├── Enums/                ClubVisibility, BadgeCategory, LiveStatus, etc.
├── Data/AppDbContext.cs  EF Core DbContext with all DbSets
├── Migrations/           10 EF Core migrations
├── Hubs/                 SignalR hubs (ChatHub, GroupHub, NotificationHub, LiveEventHub, AppHub)
├── Helpers/              JwtHelper, ClubPermissionHelper, PaginationHelper, ApiResponse
├── Filters/              ValidationFilter for model validation
├── MiddleWares/          ExceptionMiddleware for global error handling
├── wwwroot/uploads/       Locally stored uploads (voice messages, PDFs, etc.)
└── Program.cs             Application configuration and entry point
```

---

## Features

### Authentication & User
- Register, login, logout
- Email verification with token
- Forgot password and reset password via SMTP email
- Access token and refresh token system
- Profile updates (bio, department, batch, cover photo, profile image)
- Account deactivation/deletion and profile privacy (public/private)
- User search, stats, and profile view tracking

### Club Management
- Create, update, and delete clubs
- Join/leave clubs, member list, role management (Admin/Moderator/Member)
- Club privacy (public/private) and invite system (create/accept/decline/revoke)
- Club search, "my clubs", and membership status

### Post, Comment & Reaction (Social Feed)
- Create, update, and delete posts with images/videos
- Comments with nested replies, comment likes
- Reaction system (like, love, etc.) with summaries and counts
- Save/bookmark posts, report posts
- Trending posts

### Feed
- Global feed, trending feed
- Personalized feed based on followed clubs and users
- Following-only feed, club-specific feed, user-specific feed

### Event
- Create, update, and delete events
- Join/leave, join-request approval workflow (approve/reject)
- Upcoming events, club-based events, my events, joined events
- Attendee list and event stats

### Live Event (Real-Time, SignalR)
- Start/end live events
- Live chat and viewer list
- Moderation: mute, kick, and unban users

### Messaging
- Direct messages (send, edit, delete for me/everyone, seen status)
- Group chat (create, add/remove members, set admins)
- Voice messages (direct and group)
- Conversation list, unread count, message search

### Notification
- Real-time notifications via SignalR (NotificationHub)
- Unread count, mark as read/mark all as read, delete/delete all

### Follow System
- Follow/unfollow users
- Followers/following lists
- Mutual followers and suggestions
- Block/unblock users

### Story
- Create stories (images/videos), story feed
- Story view tracking and viewer list

### Poll
- Create polls within clubs, vote, close polls

### File Management
- File upload/download with type filtering
- File search and stats

### Badge & Leaderboard
- Badge catalog, owned badges, badge progress
- Club-based and global leaderboards
- Top contributor recalculation
- Badge holder list and badge revocation (admin)

### Recruitment
- Apply to clubs, withdraw applications
- Approve/reject applications (club admin)
- Pending application count

### AI Recommendation (Gemini)
- Recommended clubs, events, and people
- AI-generated smart digest
- Dismiss recommendations

### Search
- Global search across all entities, advanced search with filters
- Search suggestions, trending searches, recent search history

### Dashboard
- Summary, recent posts/clubs, trending content, stats
- AI-generated insights

### Presence
- Online/offline status tracking (with bulk check)
- Online-following list

---

## SignalR Real-Time Hubs

| Hub | Endpoint | Purpose |
|---|---|---|
| ChatHub | /hubs/chat | Real-time direct messaging |
| GroupHub | /hubs/group | Real-time group chat |
| NotificationHub | /hubs/notification | Real-time notification push |
| LiveEventHub | /hubs/live | Live event chat and viewer updates |

The JWT token must be passed via query string (`?access_token=...`) for SignalR connections, since headers cannot be sent during the WebSocket handshake. This is handled in the `OnMessageReceived` event in `Program.cs`.

---

## Database

- **SQL Server**, using EF Core with a code-first approach
- 10 migrations: Auth fix, Poll system, Recruitment, Story system, Club recommendation, Live event moderation, Advanced search, Club privacy, Timestamp fix, Event join request
- Core entities: User, Club, ClubMember, Post, Comment, Reaction, Event, EventAttendance, Group, GroupMessage, Message, Notification, Badge, UserBadge, Poll, Story, ClubApplication, FileResource, Follow, BlockedUser, and more

---

## Configuration

Set the following configuration in `appsettings.json`, or preferably in User Secrets or environment variables since these values are sensitive:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=UniversityClubDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "UniversityClubAPI",
    "Audience": "UniversityClubAPIUsers"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Email": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-1.5-flash"
  }
}
```

Never commit real secrets to GitHub. Add `appsettings.json` to `.gitignore` or use `dotnet user-secrets` for local development, and environment variables in production.

---

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server (local or remote instance)

### Installation

```bash
git clone https://github.com/Tamimkhan7/University_Club_Api_Project.git
cd University_Club_Api_Project/UniversityClubAPI/UniversityClubAPI/UniversityClubAPI
dotnet restore
```

### Configuration

Update `appsettings.json` with your own configuration values as shown above.

### Database Setup

```bash
dotnet ef database update
```

### Run

```bash
dotnet run
```

Once running, open the Swagger UI to explore and test all endpoints, typically at:

```
https://localhost:<port>/swagger
```

---

## Authentication Flow

1. `POST /api/auth/register` — register a new account (sends an email verification link)
2. `GET /api/auth/verify-email?token=...` — verify the email address
3. `POST /api/auth/login` — returns an access token and a refresh token
4. Include the access token in the header of every protected request:
   `Authorization: Bearer <access_token>`
5. `POST /api/auth/refresh-token` — obtain a new access token when the current one expires
6. Role-based authorization policies: `AdminOnly`, `ModeratorOnly`

---

## Security & Technical Highlights

- Passwords are hashed with BCrypt and never stored in plain text
- Rate limiting: 300 requests per minute per user/IP
- Global exception middleware for consistent error responses
- Validation filter for clean, structured validation error responses
- CORS policy restricting access to allowed frontend origins
- Pagination helper for consistent pagination across all list endpoints
- Consistent API response wrapper (`ApiResponse` helper class)

---

## Frontend

The corresponding frontend for this API is available at:
[University Club API Frontend](https://github.com/Tamimkhan7/University_Club_Api_Project_Frontend)

---

## License

No license has been specified for this project yet. Consider adding an MIT License if you intend to open source it.

---

## Author

**Tamim Khan** — [GitHub Profile](https://github.com/Tamimkhan7)
