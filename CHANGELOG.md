# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- JWT secret externalization via environment variable (`JWT_SECRET`) or configuration (`Jwt:Secret`)
- Example configuration files (`.env.example`, `docker-compose.example.yml`)
- `CONTRIBUTING.md` - Contributor guidelines
- `CHANGELOG.md` - Project changelog

### Security

- **BREAKING**: JWT signing key is no longer hardcoded. Must be set via `JWT_SECRET` environment variable or `Jwt:Secret` configuration value before running.

## [0.1.0] - 2026-06-12

### Initial Release

#### Features

- JWT-based authentication (register/login)
- Full task CRUD with pagination and filtering
- AI-powered task creation via Ollama LLM
- Autonomous agent system with planner-executor architecture
- Asynchronous agent job processing with retry logic
- "Dream Mode" idle-time optimization
- React + Vite frontend with SPA routing
- PostgreSQL database with EF Core migrations
- Docker Compose deployment
- Swagger/OpenAPI documentation

#### Projects

- `AiTaskApi` - Main REST API (ASP.NET Core 10)
- `AiTaskApi.Shared` - Shared DTOs and models
- `AiTaskApi.Worker` - Background worker service
- `AiTaskApi.Tests` - Unit and integration tests
- Frontend - React application

#### Documentation

- Architecture overview with mermaid diagrams
- Installation and configuration guides
- API reference with examples
- Usage workflows
- Troubleshooting guide
- FAQ
- Deployment guide (Docker, bare-metal, cloud)
- Frontend documentation
- Development guide
