# University Club API — Frontend

A full-featured React frontend for the **University Club API**, built as a university club management and social networking platform. It covers clubs, posts and feeds, events, live events, direct and group messaging, notifications, stories, polls, recruitment, badges, leaderboards, AI-powered recommendations, and real-time presence.

This is the client application that consumes the [University Club API](https://github.com/Tamimkhan7/University_Club_Api_Project) backend.

---

## Tech Stack

- **React 19** — UI library
- **Vite** — build tool and dev server
- **React Router v7** — client-side routing
- **Tailwind CSS** — styling
- **Axios** — HTTP client with interceptors for auth and token refresh
- **SignalR (@microsoft/signalr)** — real-time chat, notifications, and live events
- **React Hot Toast** — toast notifications
- **Lucide React / React Icons** — icon sets
- **ESLint** — linting

---

## Project Structure

```
university-club-frontend/
├── public/
├── src/
│   ├── api/            API service modules and Axios instance
│   ├── assets/         Images and static assets
│   ├── components/     Reusable UI components
│   ├── context/         React context providers (Auth, Presence)
│   ├── hooks/           Custom React hooks
│   ├── pages/           Route-level page components
│   ├── utils/           Utility/helper functions
│   ├── App.jsx           Route definitions
│   └── main.jsx          Application entry point
├── package.json
└── vite.config.js
```

---

## Features

### Authentication
- Register, login, logout
- Email verification
- Forgot password and reset password flows
- Automatic access token refresh with Axios interceptors
- Protected routes for authenticated users

### Feed & Posts
- Global, trending, and personalized feed
- Create, edit, and delete posts with media
- Comments, replies, and reactions
- Post save/bookmark and reporting

### Clubs
- Browse, search, and view club details
- Join/leave clubs, manage membership
- Club privacy settings, invitations, and invite management
- Club-based recruitment and application system

### Events
- Create, browse, and join events
- Join-request approval workflow
- Live event pages with real-time chat via SignalR

### Messaging
- Direct messages and group messaging
- Voice message recording and playback
- Real-time delivery via SignalR
- Conversation list with unread counts

### Notifications
- Real-time notifications via SignalR
- Mark as read, mark all as read, delete

### Stories & Polls
- Create and view 24-hour stories with viewer tracking
- Create and vote on club polls

### Social Graph
- Follow/unfollow users
- Connections, mutual followers, and suggestions
- User search and profiles

### Gamification
- Badges and badge progress
- Club-based and global leaderboards

### AI Recommendations
- Personalized club, event, and people recommendations
- AI-generated dashboard insights

### Presence & Search
- Real-time online/offline presence tracking
- Global and advanced search with suggestions

---

## Getting Started

### Prerequisites

- Node.js 18 or later
- npm
- A running instance of the [University Club API](https://github.com/Tamimkhan7/University_Club_Api_Project) backend

### Installation

```bash
git clone https://github.com/Tamimkhan7/University_Club_Api_Project_Frontend.git
cd University_Club_Api_Project_Frontend/university-club-frontend
npm install
```

### Configuration

The API base URL is currently set in `src/api/axios.js` and `src/api/signalr.js`:

```
http://localhost:5000/api
```

Update this value to match the address of your running backend instance if it differs.

### Running the App

```bash
npm run dev
```

The app will be available at `http://localhost:5173` by default.

### Building for Production

```bash
npm run build
npm run preview
```

### Linting

```bash
npm run lint
```

---

## Available Routes

| Route | Description |
|---|---|
| `/login` | User login |
| `/register` | User registration |
| `/verify-email` | Email verification |
| `/forgot-password` | Request password reset |
| `/reset-password` | Reset password |
| `/` | Main feed |
| `/dashboard` | Dashboard with stats and insights |
| `/recommendations` | AI-powered recommendations |
| `/leaderboard` | Club and global leaderboards |
| `/search` | Global search |
| `/clubs` | Browse clubs |
| `/clubs/:id` | Club details |
| `/events` | Browse events |
| `/events/:eventId/live` | Live event page |
| `/users` | Browse users |
| `/messages` | Direct messages |
| `/groups` | Group chats |
| `/files` | File management |
| `/connections` | Followers and following |
| `/applications` | My club applications |
| `/invites` | My invites |
| `/invites/:inviteId` | Invite details |
| `/notifications` | Notifications |
| `/profile` | Own profile |
| `/profile/:id` | Another user's profile |
| `/post/:id` | Post details |

---

## Real-Time Communication

The app connects to the following SignalR hubs on the backend:

| Hub | Purpose |
|---|---|
| `/hubs/chat` | Direct messaging |
| `/hubs/group` | Group messaging |
| `/hubs/notification` | Real-time notifications |
| `/hubs/live` | Live event chat and viewer updates |

Connections are authenticated using the JWT access token stored in local storage.

---

## Backend

This frontend is designed to work with the [University Club API](https://github.com/Tamimkhan7/University_Club_Api_Project), an ASP.NET Core Web API backend. Make sure the backend is running and reachable before starting the frontend.

---

## Author

**Tamim Khan** — [GitHub Profile](https://github.com/Tamimkhan7)
