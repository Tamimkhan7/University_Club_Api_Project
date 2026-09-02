# University Club Platform — Frontend

A full-featured University Club Management Platform frontend where students can join clubs, participate in events, share posts, chat and get notified in real time, and much more. Built with React and Vite, connected to a REST + real-time API backend.

This repository contains only the frontend. The backend lives in a separate repository:
https://github.com/Tamimkhan7/University_Club_Api_Project

## Features

### Authentication and Account
- Login and Register with email and password
- Email verification flow
- Forgot password and reset password flow
- Protected routes so unauthenticated users cannot access private pages

### Club Management
- Browse all clubs
- View detailed club pages
- Club privacy settings, public or private
- Invite system with invite cards and invite modals
- My Invites and Invite Details pages

### Social Feed
- News feed with post creation, viewing, and post details
- Stories with a creation modal, stories bar, and story viewer
- Polls with poll creation and voting

### Events
- Event listing and details
- Live event tracking in real time

### Messaging and Communication
- Real-time messaging
- Voice messages with a recorder bar and voice message bubbles
- Real-time notifications
- Online presence tracking

### Community
- Create and manage groups
- Connect with other users
- User profiles and a users directory
- Recruitment system with applications and application tracking

### Additional Features
- Dashboard with summary and stats
- Personalized recommendations
- Leaderboard for top contributors
- Search across clubs, users, and posts
- File sharing

## Tech Stack

| Category | Tool |
|---|---|
| Framework | React 19 |
| Build Tool | Vite |
| Routing | React Router v7 |
| Styling | Tailwind CSS |
| HTTP Client | Axios |
| Real-time | SignalR |
| Toast Notifications | react-hot-toast |
| Icons | lucide-react, react-icons |
| Language | JavaScript with some TypeScript |
| Linting | ESLint |

## Project Structure

```
university-club-frontend/
├── src/
│   ├── api/
│   ├── components/
│   │   ├── ClubPrivacy/
│   │   ├── Poll/
│   │   ├── Recruitment/
│   │   └── Story/
│   ├── context/
│   ├── hooks/
│   ├── pages/
│   ├── utils/
│   ├── App.jsx
│   └── main.jsx
├── public/
├── index.html
├── package.json
├── tailwind.config.js
└── vite.config.js
```

## Getting Started

### Prerequisites

- Node.js 18 or higher
- npm

### Installation

```bash
git clone https://github.com/Tamimkhan7/University_Club_Api_Project_Frontend.git
cd University_Club_Api_Project_Frontend/university-club-frontend
npm install
npm run dev
```

The app will be available at `http://localhost:5173`.

### Available Scripts

```bash
npm run dev
npm run build
npm run preview
npm run lint
```

## Backend Configuration

The API base URL is currently set in `src/api/axios.js`. By default it points to:

```
http://localhost:5000/api
```

Make sure the backend is running locally on port 5000, or update the `API_BASE_URL` value in `src/api/axios.js` to point to your deployed backend.

Backend repository:
https://github.com/Tamimkhan7/University_Club_Api_Project

## Screenshots

Add screenshots or GIFs of key pages here, such as the feed, clubs, live events, and messages.

## Developer

Tamim Khan
GitHub: https://github.com/Tamimkhan7

## License

This project currently has no license. Add an MIT or other license if needed.
