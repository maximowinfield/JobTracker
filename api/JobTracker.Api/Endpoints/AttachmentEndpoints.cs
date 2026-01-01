// ================================
// ATTACHMENT_ENDPOINTS_S3_PRESIGNED_DROPIN
// ================================
using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Endpoints;

public static class AttachmentEndpoints
{
    // 🔎 SEARCH TERM: ATTACHMENTS_TO_DTO
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

    // 🔎 SEARCH TERM: ATTACHMENTS_PRESIGN_UPLOAD_REQUEST
    public record PresignUploadRequest(
        string FileName,
        string ContentType,
        long SizeBytes
    );

    // 🔎 SEARCH TERM: ATTACHMENTS_PRESIGN_UPLOAD_RESPONSE
    public record PresignUploadResponse(
        string UploadUrl,
        string StorageKey,
        int ExpiresInSeconds
    );

    // 🔎 SEARCH TERM: ATTACHMENTS_PRESIGN_DOWNLOAD_RESPONSE
    public record PresignDownloadResponse(
        string DownloadUrl,
        int ExpiresInSeconds
    );

    // 🔎 SEARCH TERM: ATTACHMENTS_CREATE_ATTACHMENT_REQUEST
    public record CreateAttachmentRequest(
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey
    );

    public static void MapAttachments(this WebApplication app)
    {
        // 🔎 SEARCH TERM: ATTACHMENTS_GROUP_SETUP
        var group = app.MapGroup("/api")
            .RequireAuthorization();

        // ================================
        // 🔎 SEARCH TERM: ATTACHMENTS_PRESIGN_UPLOAD_ENDPOINT
        // POST /api/job-apps/{jobAppId}/attachments/presign-upload
        // Returns a presigned PUT URL so the frontend can upload directly to S3.
        // ================================
        group.MapPost("/job-apps/{jobAppId:int}/attachments/presign-upload", async (
            int jobAppId,
            PresignUploadRequest req,
            AppDbContext db,
            ClaimsPrincipal user,
            IAmazonS3 s3) =>
        {
            const long MaxFileSizeBytes = 25L * 1024L * 1024L; // 25 MB
            const int ExpSeconds = 10 * 60; // 10 minutes

            if (string.IsNullOrWhiteSpace(req.FileName))
                return Results.BadRequest(new { error = "FileName is required." });

            if (req.SizeBytes <= 0)
                return Results.BadRequest(new { error = "SizeBytes must be > 0." });

            if (req.SizeBytes > MaxFileSizeBytes)
                return Results.BadRequest(new { error = "File too large (max 25 MB)." });

            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // ✅ ensure the job app belongs to this user
            var ownsApp = await db.JobApplications
                .AnyAsync(x => x.Id == jobAppId && x.UserId == userId);

            if (!ownsApp) return Results.NotFound();

            var bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
            if (string.IsNullOrWhiteSpace(bucket))
                return Results.Problem("S3_BUCKET_NAME is not set.");

            // 🔎 SEARCH TERM: ATTACHMENTS_S3_KEY_FORMAT
            var safeFileName = Path.GetFileName(req.FileName.Trim());
            var guid = Guid.NewGuid().ToString("N");

            // Keep keys user-scoped + app-scoped for easier authorization checks
            var storageKey = $"users/{userId}/job-apps/{jobAppId}/attachments/{guid}/{safeFileName}";

            // Generate presigned PUT URL
            var presign = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = storageKey,
                Verb = HttpVerb.PUT,
                ContentType = req.ContentType ?? "application/octet-stream",
                Expires = DateTime.UtcNow.AddSeconds(ExpSeconds)
            };

            var uploadUrl = s3.GetPreSignedURL(presign);

            return Results.Ok(new PresignUploadResponse(
                uploadUrl,
                storageKey,
                ExpSeconds
            ));
        });

        // ================================
        // 🔎 SEARCH TERM: ATTACHMENTS_LIST_FOR_JOBAPP
        // GET /api/job-apps/{jobAppId}/attachments
        // ================================
        group.MapGet("/job-apps/{jobAppId:int}/attachments", async (
            int jobAppId,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ownsApp = await db.JobApplications
                .AnyAsync(x => x.Id == jobAppId && x.UserId == userId);

            if (!ownsApp) return Results.NotFound();

            var attachments = await db.Attachments
                .Where(a => a.JobApplicationId == jobAppId
                         && a.JobApplication!.UserId == userId)
                .OrderByDescending(a => a.CreatedAtUtc)
                .Select(a => ToDto(a))
                .ToListAsync();


            return Results.Ok(attachments);
        });

        // ================================
        // 🔎 SEARCH TERM: ATTACHMENTS_CREATE_METADATA_ONLY
        // POST /api/job-apps/{jobAppId}/attachments (metadata only)
        // Expects StorageKey from the presign endpoint.
        // ================================
        group.MapPost("/job-apps/{jobAppId:int}/attachments", async (
            int jobAppId,
            CreateAttachmentRequest req,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ownsApp = await db.JobApplications
                .AnyAsync(x => x.Id == jobAppId && x.UserId == userId);

            if (!ownsApp) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(req.FileName))
                return Results.BadRequest(new { error = "FileName is required." });

            if (req.SizeBytes <= 0)
                return Results.BadRequest(new { error = "SizeBytes must be > 0." });

            if (string.IsNullOrWhiteSpace(req.StorageKey))
                return Results.BadRequest(new { error = "StorageKey is required (from presign-upload)." });

            // ✅ Guardrail: ensure the storage key is within this user's prefix
            // 🔎 SEARCH TERM: ATTACHMENTS_STORAGEKEY_OWNERSHIP_CHECK
            var expectedPrefix = $"users/{userId}/job-apps/{jobAppId}/attachments/";
            if (!req.StorageKey.StartsWith(expectedPrefix, StringComparison.Ordinal))
                return Results.BadRequest(new { error = "Invalid StorageKey for this user/job application." });

            var attachment = new Attachment
            {
                JobApplicationId = jobAppId,
                FileName = req.FileName.Trim(),
                ContentType = req.ContentType?.Trim() ?? "",
                SizeBytes = req.SizeBytes,
                StorageKey = req.StorageKey.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Attachments.Add(attachment);
            await db.SaveChangesAsync();

            return Results.Created($"/api/attachments/{attachment.Id}", ToDto(attachment));
        });

        // ================================
        // 🔎 SEARCH TERM: ATTACHMENTS_PRESIGN_DOWNLOAD_ENDPOINT
        // GET /api/attachments/{id}/presign-download
        // Returns a presigned GET URL to download the private S3 object.
        // ================================
        group.MapGet("/attachments/{id:int}/presign-download", async (
            int id,
            AppDbContext db,
            ClaimsPrincipal user,
            IAmazonS3 s3) =>
        {
            const int ExpSeconds = 10 * 60; // 10 minutes
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var attachment = await db.Attachments
                .Include(a => a.JobApplication!)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobApplication!.UserId == userId);

            if (attachment is null) return Results.NotFound();

            var bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
            if (string.IsNullOrWhiteSpace(bucket))
                return Results.Problem("S3_BUCKET_NAME is not set.");

            var presign = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = attachment.StorageKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddSeconds(ExpSeconds)
            };

            var downloadUrl = s3.GetPreSignedURL(presign);

            return Results.Ok(new PresignDownloadResponse(downloadUrl, ExpSeconds));
        });

        // ================================
        // 🔎 SEARCH TERM: ATTACHMENTS_DELETE_ENDPOINT_WITH_S3_DELETE
        // DELETE /api/attachments/{id}
        // Deletes DB metadata + best-effort deletes the S3 object.
        // ================================
        group.MapDelete("/attachments/{id:int}", async (
            int id,
            AppDbContext db,
            ClaimsPrincipal user,
            IAmazonS3 s3) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var attachment = await db.Attachments
                .Include(a => a.JobApplication!)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobApplication!.UserId == userId);

            if (attachment is null) return Results.NotFound();

            var bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
            if (!string.IsNullOrWhiteSpace(bucket) && !string.IsNullOrWhiteSpace(attachment.StorageKey))
            {
                try
                {
                    await s3.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = bucket,
                        Key = attachment.StorageKey
                    });
                }
                catch
                {
                    // Best-effort: we still remove metadata even if S3 delete fails.
                }
            }

            db.Attachments.Remove(attachment);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
