# Contributing to AiTaskApi

Thank you for your interest in AiTaskApi! This document provides guidelines and instructions for contributing.

## How to Contribute

### Reporting Bugs

Before creating a bug report, please check existing issues. When creating an issue:

1. Use a clear and descriptive title
2. Describe the exact steps to reproduce the problem
3. Provide expected vs. actual behavior
4. Include configuration details (environment, version, etc.)

### Suggesting Features

Feature requests are welcome. Please include:

1. The problem you're trying to solve
2. Proposed solution
3. Alternative solutions considered

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make focused, atomic changes
4. Update documentation if behavior changes
5. Ensure all tests pass (`dotnet test`)
6. Submit a pull request with a clear description

## Code Standards

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `TaskService` |
| Methods | PascalCase | `GetAll()` |
| Properties | PascalCase | `Title` |
| Private fields | `_camelCase` | `_context` |
| DTOs | Suffix `Dto` | `TaskCreateDto` |
| Services | Suffix `Service` | `TaskService` |
| Interfaces | Prefix `I` | `IAiService` |

### Code Style

- Follow [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `var` when the type is obvious
- Keep methods focused and small
- Add XML documentation for public APIs

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add agent retry configuration
fix: resolve null reference in task service
docs: update API reference
chore: update dependencies
```

## Development Setup

See [`docs/development.md`](./docs/development.md) for full onboarding instructions.

Quick start:

```bash
# Clone and setup
git clone https://github.com/yourusername/AiTaskApi.git
cd backend
dotnet restore

# Configure database
# Edit AiTaskApi/appsettings.Development.json with your connection string

# Run
cd AiTaskApi
dotnet run

cd ../AiTaskApi.Worker
dotnet run
```

## Testing

```bash
# Run all tests
dotnet test AiTaskApi.Tests/AiTaskApi.Tests.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~TaskServiceTests"
```

## Documentation

- Update [`docs/`](./docs/) when behavior changes
- Keep API reference in sync with actual endpoints
- Use relative links for internal documentation

## Security

**Never commit secrets, credentials, or sensitive configuration.**

Use environment variables or the `.env.example` files for configuration:

```bash
# Copy and configure
cp backend/.env.example backend/.env.local
cp frontend/.env.example frontend/.env.local
```

## Code of Conduct

- Be respectful and constructive
- Provide constructive feedback
- Focus on the code, not the person
- Help others learn and grow

## Questions?

Open an issue or refer to:

- [`docs/faq.md`](./docs/faq.md) - Frequently asked questions
- [`docs/troubleshooting.md`](./docs/troubleshooting.md) - Common issues
- [`docs/architecture.md`](./docs/architecture.md) - System design
