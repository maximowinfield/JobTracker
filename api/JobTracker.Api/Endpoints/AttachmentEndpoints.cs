using System.Security.Claims;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Endpoints;

public static class AttachmentEndpoints
{
    private static AttachmentDto ToDto(Attachment a) =>
        new(
            a.Id,
            a.JobApplicationId,
            a.FileName,
            a.ContentType,
            a.SizeBytes,
            a.StorageKey,
            a.CreatedAtUtc
        );

    public record CreateAttachmentRequest(
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey
    );

    public static void MapAttachments(this WebApplication app)
    {
        // All routes require auth, same as your job-app endpoints pattern
        var group = app.MapGroup("/api")
            .RequireAuthorization();

        // GET /api/job-apps/{jobAppId}/attachments
        group.MapGet("/job-apps/{jobAppId:int}/attachments", async (
            int jobAppId,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // ✅ ensure the job app belongs to this user
            var ownsApp = await db.JobApplications
                .AnyAsync(x => x.Id == jobAppId && x.UserId == userId);

            if (!ownsApp) return Results.NotFound();

            var attachments = await db.Attachments
                .Where(a => a.JobApplicationId == jobAppId)
                .OrderByDescending(a => a.CreatedAtUtc)
                .Select(a => ToDto(a))
                .ToListAsync();

            return Results.Ok(attachments);
        });

        // POST /api/job-apps/{jobAppId}/attachments   (metadata only)
        group.MapPost("/job-apps/{jobAppId:int}/attachments", async (
            int jobAppId,
            CreateAttachmentRequest req,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // ✅ ensure the job app belongs to this user
            var ownsApp = await db.JobApplications
                .AnyAsync(x => x.Id == jobAppId && x.UserId == userId);

            if (!ownsApp) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(req.FileName))
                return Results.BadRequest(new { error = "FileName is required." });

            if (req.SizeBytes <= 0)
                return Results.BadRequest(new { error = "SizeBytes must be > 0." });

            var attachment = new Attachment
            {
                JobApplicationId = jobAppId,
                FileName = req.FileName.Trim(),
                ContentType = req.ContentType?.Trim() ?? "",
                SizeBytes = req.SizeBytes,
                StorageKey = req.StorageKey?.Trim() ?? "",
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Attachments.Add(attachment);
            await db.SaveChangesAsync();

            return Results.Created($"/api/attachments/{attachment.Id}", ToDto(attachment));
        });

        // DELETE /api/attachments/{id}
        group.MapDelete("/attachments/{id:int}", async (
            int id,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // ✅ join to JobApplications to enforce ownership
            var attachment = await db.Attachments
                .Include(a => a.JobApplication!)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobApplication!.UserId == userId);

            if (attachment is null) return Results.NotFound();

            db.Attachments.Remove(attachment);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
