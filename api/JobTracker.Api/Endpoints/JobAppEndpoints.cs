using System.Security.Claims;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Dtos;

namespace JobTracker.Api.Endpoints;

/**
 * JobAppEndpoints
 * - Purpose: Minimal API endpoints for CRUD + filtering/paging of job applications.
 * - Interview talking point: Every request is scoped to the authenticated user (ownership enforcement).
 */
public static class JobAppEndpoints
{
    // Map EF entity -> DTO so API contract is stable and doesn't leak persistence concerns
    private static JobAppDto ToDto(JobApplication x) =>
        new(
            x.Id,
            x.Company,
            x.RoleTitle,
            x.Status,
            x.Notes,
            x.CreatedAtUtc,
            x.UpdatedAtUtc
        );

    public static void MapJobApps(this WebApplication app)
    {
        // All endpoints in this group require authentication
        var group = app.MapGroup("/api/job-apps")
            .RequireAuthorization();

        /**
         * GET /api/job-apps?q=amazon&status=Applied&page=1&pageSize=25
         * - Purpose: List applications for the current user with optional search + status filter + pagination.
         * - Interview talking point: AsNoTracking improves read performance; paging prevents large result sets.
         */
        group.MapGet("", async (
            AppDbContext db,
            ClaimsPrincipal user,
            string? q,
            ApplicationStatus? status,
            int page = 1,
            int pageSize = 25) =>
        {
            // Authenticated user's ID comes from JWT claims
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Guardrails for paging input
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            // Ownership enforcement: only fetch apps belonging to the current user
            IQueryable<JobApplication> query = db.JobApplications
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            // Text search across Company, RoleTitle, Notes (LIKE query)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = $"%{q.Trim()}%";

                query = query.Where(x =>
                    (x.Company != null && EF.Functions.Like(x.Company, term)) ||
                    (x.RoleTitle != null && EF.Functions.Like(x.RoleTitle, term)) ||
                    (x.Notes != null && EF.Functions.Like(x.Notes, term))
                );
            }

            // Optional status filter (maps to Kanban lanes)
            if (status is not null)
                query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToDto(x))
                .ToListAsync();

            return Results.Ok(new PagedResult<JobAppDto>(items, total, page, pageSize));
        });

        /**
         * POST /api/job-apps
         * - Purpose: Create a new job application owned by the current user.
         */
        group.MapPost("", async (AppDbContext db, ClaimsPrincipal user, CreateJobAppRequest req) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (string.IsNullOrWhiteSpace(req.Company) || string.IsNullOrWhiteSpace(req.RoleTitle))
                return Results.BadRequest("Company and roleTitle are required.");

            var entity = new JobApplication
            {
                UserId = userId,
                Company = req.Company.Trim(),
                RoleTitle = req.RoleTitle.Trim(),
                Status = req.Status ?? ApplicationStatus.Draft,
                Notes = req.Notes,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            db.JobApplications.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created($"/api/job-apps/{entity.Id}", ToDto(entity));
        });

        /**
         * PATCH /api/job-apps/{id}
         * - Purpose: Partial updates (ideal for Kanban moves: status change only).
         * - Interview talking point: We enforce ownership by selecting by (id AND userId).
         */
        group.MapPatch("{id:int}", async (AppDbContext db, ClaimsPrincipal user, int id, UpdateJobAppRequest req) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Ownership enforcement
            var entity = await db.JobApplications
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (entity is null) return Results.NotFound();

            if (req.Company is not null) entity.Company = req.Company.Trim();
            if (req.RoleTitle is not null) entity.RoleTitle = req.RoleTitle.Trim();
            if (req.Status is not null) entity.Status = req.Status.Value;
            if (req.Notes is not null) entity.Notes = req.Notes;

            entity.UpdatedAtUtc = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(ToDto(entity));
        });

        /**
         * DELETE /api/job-apps/{id}
         * - Purpose: Delete an application owned by the current user.
         */
        group.MapDelete("{id:int}", async (AppDbContext db, ClaimsPrincipal user, int id) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var entity = await db.JobApplications
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (entity is null) return Results.NotFound();

            db.JobApplications.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    // Request DTOs used by POST/PATCH endpoints
    public record CreateJobAppRequest(
        string Company,
        string RoleTitle,
        ApplicationStatus? Status,
        string? Notes
    );

    public record UpdateJobAppRequest(
        string? Company,
        string? RoleTitle,
        ApplicationStatus? Status,
        string? Notes
    );
}
