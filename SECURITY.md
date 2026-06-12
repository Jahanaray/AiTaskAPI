# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 0.1.x   | Yes       |

## Reporting a Vulnerability

We take the security of AiTaskApi seriously. If you discover a security vulnerability, please follow these steps:

### Do NOT

- Open a public GitHub issue
- Discuss the vulnerability in public channels

### Do

1. **Email** the details to the repository maintainers
2. **Encrypt** your message if possible (provide your PGP key)
3. **Include** detailed reproduction steps
4. **Allow** time for resolution before any disclosure

### What to Include

- Description of the vulnerability
- Steps to reproduce
- Potential impact assessment
- Suggested fix (if any)

## Security Measures in Place

| Measure | Implementation |
|---------|----------------|
| Password hashing | BCrypt.Net-Next |
| Authentication | JWT Bearer tokens |
| HTTPS | HTTPS redirection enabled |
| CORS | Origin whitelist configuration |
| Secrets management | Environment variables (`.env` files) |

## Security Best Practices for Users

1. **Never commit `.env` files** - They may contain secrets
2. **Use strong passwords** - Minimum 12 characters with mixed case, numbers, symbols
3. **Rotate JWT secrets** - Change `JWT_SECRET` periodically
4. **Keep dependencies updated** - Run `dotnet update` and `npm update` regularly
5. **Use HTTPS in production** - Never expose the API over plain HTTP
6. **Configure CORS properly** - Only allow trusted origins
7. **Use strong database passwords** - Never use default credentials

## Known Security Considerations

| Item | Status | Notes |
|------|--------|-------|
| JWT secret externalization | Complete | Configurable via `JWT_SECRET` env var |
| Rate limiting | Not implemented | Recommended for production |
| Input validation | Basic | DTO constraints only; consider FluentValidation |
| Audit logging | Not implemented | Track user actions for compliance |
| Token refresh | Not implemented | Tokens expire after 2 hours without refresh |

## Dependencies Security

- **Backend**: Monitor [.NET releases](https://dotnet.microsoft.com/download/dotnet) for security updates
- **Frontend**: Run `npm audit` regularly and update dependencies
- **Docker**: Use specific image tags, not `latest`
- **PostgreSQL**: Keep PostgreSQL updated to receive security patches
