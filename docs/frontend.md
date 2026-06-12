# Frontend Documentation

## Overview

The AiTaskApi frontend is a React 18 application built with Vite, providing a web interface for task management and AI interactions.

## Project Structure

```
frontend/
├── src/
│   ├── App.css                 # Main application styles
│   ├── App.jsx                 # Root component with routing
│   ├── index.css               # Global styles and CSS variables
│   ├── main.jsx                # Application entry point (React DOM render)
│   ├── api/
│   │   └── api.js              # Axios HTTP client configuration
│   └── assets/
│       ├── hero.png            # Hero section image
│       ├── react.svg           # React branding asset
│       └── vite.svg            # Vite branding asset
├── public/
│   ├── favicon.svg             # Browser tab icon
│   └── icons.svg               # SVG sprite sheet
├── Dockerfile                  # Frontend container build
├── nginx.conf                  # Nginx reverse proxy configuration
├── vite.config.js              # Vite build configuration
├── eslint.config.js            # ESLint rules
├── package.json                # Dependencies and scripts
└── .env.development            # Development environment variables
└── .env.production             # Production environment variables
```

## Technology Stack

| Technology | Purpose | Version |
|------------|---------|---------|
| React 18 | UI framework | Latest |
| Vite | Build tool and dev server | Latest |
| Axios | HTTP client for API calls | Latest |
| CSS3 | Styling | Native |
| ESLint | Code quality | Latest |

## Environment Configuration

### Development (`.env.development`)

```bash
VITE_API_URL=http://localhost:5058/api
```

### Production (`.env.production`)

```bash
VITE_API_URL=https://your-api-domain.com/api
```

## Available Scripts

```bash
# Start development server (hot reload on port 5173)
npm run dev

# Build for production (outputs to dist/)
npm run build

# Preview production build locally
npm run preview

# Run ESLint
npm run lint
```

## API Client Configuration

The frontend uses a centralized Axios instance in [`src/api/api.js`](../frontend/src/api/api.js):

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5058/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add JWT token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

## Key Components

### Authentication Flow

1. User enters credentials on login/register form
2. Credentials sent to `/api/auth/login` or `/api/auth/register`
3. JWT token received and stored in `localStorage`
4. Token automatically attached to all subsequent API requests via interceptor

### Task Management

The frontend provides CRUD operations for tasks:

| Operation | API Endpoint | Method |
|-----------|-------------|--------|
| List tasks | `/api/tasks` | GET |
| Get task | `/api/tasks/{id}` | GET |
| Create task | `/api/tasks` | POST |
| Update task | `/api/tasks/{id}` | PUT |
| Delete task | `/api/tasks/{id}` | DELETE |

### AI Features

| Feature | API Endpoint | Description |
|---------|-------------|-------------|
| Chat | `/api/ai/chat` | Send message, receive LLM response |
| Create task via AI | `/api/ai/create-task` | Natural language to task (async) |
| Agent execution | `/api/ai/agent-loop` | Full agent pipeline (async) |
| Job status | `/api/ai/jobs/{id}` | Check async job progress |

## Docker Deployment

### Dockerfile

```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Nginx Configuration (`nginx.conf`)

```nginx
server {
    listen 80;
    server_name localhost;
    
    root /usr/share/nginx/html;
    index index.html;
    
    # SPA routing support
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # API proxy (if needed)
    location /api/ {
        proxy_pass http://host.docker.internal:5058/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## Styling Architecture

The frontend uses CSS variables for theming:

```css
:root {
  --primary-color: #007bff;
  --secondary-color: #6c757d;
  --success-color: #28a745;
  --danger-color: #dc3545;
  --warning-color: #ffc107;
  --info-color: #17a2b8;
  --light-color: #f8f9fa;
  --dark-color: #343a40;
}
```

## CORS Configuration

The backend CORS policy in [`Program.cs`](../backend/AiTaskApi/Program.cs:52-64) allows:

| Origin | Purpose |
|--------|---------|
| `http://localhost:5173` | Vite development server |
| `http://localhost:3000` | Alternative React dev server |

For production, add your frontend domain to the CORS policy.

## Building for Production

```bash
# 1. Set production environment variables
cp .env.development .env.production
# Edit .env.production with production API URL

# 2. Build
npm run build

# 3. Output is in dist/ - deploy to any static host
#    - Netlify, Vercel, GitHub Pages
#    - Custom nginx server
#    - CDN
```

## Browser Support

| Browser | Version |
|---------|---------|
| Chrome | Latest |
| Firefox | Latest |
| Safari | Latest |
| Edge | Latest |

Vite uses native ES modules, so IE11 is not supported.
