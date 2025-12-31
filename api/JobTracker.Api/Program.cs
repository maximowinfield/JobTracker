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




var builder = WebApplication.CreateBuilder(args);

// ----- 
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ----- Database -----
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db";
    opt.UseSqlite(cs);
});

// ----- JWT Options -----
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

// ----- Auth -----
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

        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var auth = ctx.Request.Headers.Authorization.ToString();
                Console.WriteLine("AUTH HEADER RAW: " + auth);

                if (!string.IsNullOrWhiteSpace(auth) &&
                    auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = auth.Substring("Bearer ".Length).Trim().Trim('"');
                    Console.WriteLine($"EXTRACTED TOKEN dots={token.Count(c => c == '.')}, len={token.Length}");
                    ctx.Token = token; // ✅ force what gets validated
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



builder.Services.AddAuthorization();

// CORS (dev-friendly; tighten later)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

app.MapGet("/api/debug/header", (HttpContext ctx) =>
{
    return Results.Ok(new
    {
        Authorization = ctx.Request.Headers.Authorization.ToString()
    });
});


app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();


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


app.MapGet("/api/health", () => Results.Ok(new { ok = true }));

app.MapGet("/api/debug/jwt", (JwtOptions o) => Results.Ok(new
{
    Environment = app.Environment.EnvironmentName,
    o.Issuer,
    o.Audience,
    SecretLength = o.Secret.Length
}));


app.MapAuth();

app.MapJobApps();

app.MapAttachments();

app.Run();
