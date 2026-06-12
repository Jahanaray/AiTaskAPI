using AiTaskApi.Data;
using AiTaskApi.Models;
using Microsoft.EntityFrameworkCore;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Register(string username, string password)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (exists) return false;

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<User?> Login(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return null;

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        return valid ? user : null;
    }
}