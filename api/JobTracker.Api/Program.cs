using System.Text;
using JobTracker.Api.Auth;
using JobTracker.Api.Data;
using JobTracker.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using System.Text.Json.Serialization;
using Amazon.S3;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////////////////////////////////////////////////
// JSON Serialization
// - Convert enums to strings in JSON instead of integers.
// - Benefits: readability, less client-side mapping, safer API evolution.
////////////////////////////////////////////////////////////////////////////////
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

////////////////////////////////////////////////////////////////////////////////
// Database (Entity Framework Core)
// - Supports Render (PostgreSQL) in production and SQLite for local development.
// - Reads a single connection string key ("Default") and auto-selects provider.
// - This keeps deployment simple: environment config determines provider.
////////////////////////////////////////////////////////////////////////////////
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default");

    // If Render Postgres is configured, the connection string will start with "Host=" or "postgres"
    if (!string.IsNullOrWhiteSpace(cs) &&
        (cs.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
         cs.StartsWith("postgres", StringComparison.OrdinalIgnoreCase)))
    {
        // Production-like relational DB
        opt.UseNpgsql(cs);
    }
    else
    {
        // Local dev fallback (simple, file-based)
        opt.UseSqlite(cs ?? "Data Source=app.db");
    }
});

////////////////////////////////////////////////////////////////////////////////
// AWS S3 Client (Dependency Injection)
// - Singleton is appropriate: AmazonS3Client is thread-safe and reusable.
// - Region is sourced from env/config to support multiple deployment environments.
// - Used by attachments feature (presigned URLs / object operations).
////////////////////////////////////////////////////////////////////////////////
// ================================
// S3_DI_REGISTRATION_PROGRAM_CS
// ================================
builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var region = Environment.GetEnvironmentVariable("AWS_REGION")
                 ?? builder.Configuration["AWS_REGION"]
                 ?? throw new Exception("AWS_REGION is not set (env var or config).");

    return new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
});

////////////////////////////////////////////////////////////////////////////////
// JWT Options
// - Centralized config for issuer/audience/secret/expiration.
// - NOTE: In production, JWT Secret should be set via environment variables,
//   not a default literal. (Default is only for local development convenience.)
////////////////////////////////////////////////////////////////////////////////
var jwtSection = builder.Configuration.GetSection("JWT");
var jwtOpts = new JwtOptions
{
    Issuer = jwtSection["Issuer"] ?? "JobTracker",
    Audience = jwtSection["Audience"] ?? "JobTracker",
    Secret = jwtSection["Secret"] ?? "dev-only-change-me",
    ExpMinutes = int.TryParse(jwtSection["ExpMinutes"], out var m) ? m : 60
};

builder.Services.AddSingleton(jwtOpts);
builder.Services.AddSingleton<JwtTokenService>();

////////////////////////////////////////////////////////////////////////////////
// Authentication (JWT Bearer)
// - Stateless auth: client sends "Authorization: Bearer <token>" each request.
// - TokenValidationParameters enforce issuer, audience, lifetime, and signature.
// - ClockSkew set to zero for strict expiration handling (no grace period).
////////////////////////////////////////////////////////////////////////////////
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOpts.Issuer,
            ValidAudience = jwtOpts.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Secret)),
            ClockSkew = TimeSpan.Zero
        };

        ////////////////////////////////////////////////////////////////////////////
        // Debug instrumentation (useful during local development / troubleshooting)
        // - OnMessageReceived: extracts token exactly as server will validate it.
        // - OnTokenValidated / OnAuthenticationFailed: logs success/failure causes.
        //
        // NOTE: In production, reduce raw logging to avoid leaking sensitive data.
        ////////////////////////////////////////////////////////////////////////////
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var auth = ctx.Request.Headers.Authorization.ToString();
                Console.WriteLine("AUTH HEADER RAW: " + auth);

                // Some clients accidentally wrap tokens in quotes or send extra whitespace.
                // This ensures we validate the intended token.
                if (!string.IsNullOrWhiteSpace(auth) &&
                    auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = auth.Substring("Bearer ".Length).Trim().Trim('"');
                    Console.WriteLine($"EXTRACTED TOKEN dots={token.Count(c => c == '.')}, len={token.Length}");
                    ctx.Token = token; // Force what gets validated
                }

                return Task.CompletedTask;
            },

            OnTokenValidated = ctx =>
            {
                Console.WriteLine("JWT OK ✅ email=" + ctx.Principal?.FindFirst("email")?.Value);
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT FAIL ❌ " + ctx.Exception.GetType().Name + " - " + ctx.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

////////////////////////////////////////////////////////////////////////////////
// Authorization
// - Enables endpoint protection via [RequireAuthorization()] for Minimal APIs.
// - Policies/roles could be added later if/when needed.
////////////////////////////////////////////////////////////////////////////////
builder.Services.AddAuthorization();

////////////////////////////////////////////////////////////////////////////////
// CORS (Cross-Origin Resource Sharing)
// - Allows frontend (hosted separately) to call this API.
// - Current policy is dev-friendly (allows any origin). Tighten later to an
//   allow-list of known frontend domains for production hardening.
////////////////////////////////////////////////////////////////////////////////
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

////////////////////////////////////////////////////////////////////////////////
// Database migrations on startup
// - Automatically applies EF Core migrations at runtime.
// - Great for demos and small deployments.
// - For larger environments, migrations may be done in CI/CD instead.
////////////////////////////////////////////////////////////////////////////////
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

////////////////////////////////////////////////////////////////////////////////
// Static file hosting (SPA support)
// - UseDefaultFiles + UseStaticFiles allows serving the React build output.
// - MapFallbackToFile("index.html") supports client-side routing.
////////////////////////////////////////////////////////////////////////////////
app.UseDefaultFiles();
app.UseStaticFiles();

////////////////////////////////////////////////////////////////////////////////
// Debug endpoints (development helpers)
// - /api/debug/header: confirm Authorization header arrives as expected.
// - /api/debug/jwt: confirm issuer/audience config and secret length.
// - /api/debug/s3: confirm S3 env vars and credential presence.
// NOTE: These should be removed or locked down in production.
////////////////////////////////////////////////////////////////////////////////
app.MapGet("/api/debug/header", (HttpContext ctx) =>
{
    return Results.Ok(new
    {
        Authorization = ctx.Request.Headers.Authorization.ToString()
    });
});

app.UseCors("Frontend");

// Order matters:
// - Authenticate first (build user principal from token)
// - Authorize second (enforce access rules on endpoints)
app.UseAuthentication();
app.UseAuthorization();

////////////////////////////////////////////////////////////////////////////////
// Authenticated identity introspection
// - /api/me is a quick way for the frontend to verify auth status and claims.
////////////////////////////////////////////////////////////////////////////////
app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        NameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        Sub = user.FindFirst("sub")?.Value,
        Email = user.FindFirst("email")?.Value
    });
}).RequireAuthorization();

////////////////////////////////////////////////////////////////////////////////
// Health check (simple liveness probe)
////////////////////////////////////////////////////////////////////////////////
app.MapGet("/api/health", () => Results.Ok(new { ok = true }));

app.MapGet("/api/debug/jwt", (JwtOptions o) => Results.Ok(new
{
    Environment = app.Environment.EnvironmentName,
    o.Issuer,
    o.Audience,
    SecretLength = o.Secret.Length
}));

app.MapGet("/api/debug/s3", () => Results.Ok(new
{
    Region = Environment.GetEnvironmentVariable("AWS_REGION"),
    Bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME"),
    HasAccessKey = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")),
    HasSecretKey = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")),
}))
.AllowAnonymous();

////////////////////////////////////////////////////////////////////////////////
// Endpoint modules
// - Keeps Program.cs readable by mapping feature endpoints by responsibility.
// - Auth: login/register/token issuance
// - JobApps: CRUD for job application resources
// - Attachments: S3 presigned uploads + metadata
////////////////////////////////////////////////////////////////////////////////
app.MapAuth();
app.MapJobApps();
app.MapAttachments();

// SPA fallback for React Router routes (e.g., /login, /dashboard)
app.MapFallbackToFile("index.html");

app.Run();
