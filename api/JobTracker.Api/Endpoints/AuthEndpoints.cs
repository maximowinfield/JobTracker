using BCrypt.Net;
using JobTracker.Api.Auth;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Endpoints;

/**
 * AuthEndpoints
 * - Purpose: Register + login endpoints that issue JWT tokens.
 * - Interview talking point: Passwords are hashed (BCrypt) and the API returns a token on success.
 */
public static class AuthEndpoints
{
    public static void MapAuth(this WebApplication app)
    {
        /**
         * POST /api/auth/register
         * - Creates a new user
         * - Hashes password with BCrypt
         * - Returns a JWT for immediate sign-in
         */
        app.MapPost("/api/auth/register", async (RegisterRequest req, AppDbContext db, JwtTokenService jwt) =>
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var password = req.Password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Results.BadRequest("Email and password are required.");

            // Basic validation; can be expanded later (password rules, rate limiting, etc.)
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

            // Token contains user identity (id/email) used later for ownership enforcement
            var token = jwt.CreateToken(user.Id, user.Email);
            return Results.Ok(new AuthResponse(token));
        });

        /**
         * POST /api/auth/login
         * - Verifies credentials
         * - Returns a JWT token if valid
         */
        app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db, JwtTokenService jwt) =>
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var password = req.Password ?? "";

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null) return Results.Unauthorized();

            // BCrypt verify compares raw password to stored hash
            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!ok) return Results.Unauthorized();

            var token = jwt.CreateToken(user.Id, user.Email);
            return Results.Ok(new AuthResponse(token));
        });
    }
}
