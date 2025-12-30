using BCrypt.Net;
using JobTracker.Api.Auth;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this WebApplication app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest req, AppDbContext db, JwtTokenService jwt) =>
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var password = req.Password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Results.BadRequest("Email and password are required.");

            // simple sanity checks (you can harden later)
            if (password.Length < 8) return Results.BadRequest("Password must be at least 8 characters.");

            var exists = await db.Users.AnyAsync(u => u.Email == email);
            if (exists) return Results.Conflict("Email already in use.");

            var user = new AppUser
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = jwt.CreateToken(user.Id, user.Email);
            return Results.Ok(new AuthResponse(token));
        });

        app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db, JwtTokenService jwt) =>
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var password = req.Password ?? "";

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null) return Results.Unauthorized();

            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!ok) return Results.Unauthorized();

            var token = jwt.CreateToken(user.Id, user.Email);
            return Results.Ok(new AuthResponse(token));
        });
    }
}
