// ================================
// ATTACHMENT_ENDPOINTS_S3_PRESIGNED_DROPIN
// ================================
// Purpose:
// - Handles file attachments (resumes / cover letters) for job applications
// - Uses S3 presigned URLs so files upload/download directly with S3
// - API stores metadata + enforces authorization, not file bytes
// ================================
// Attachments are implemented as a separate vertical slice. The API generates presigned S3 URLs for uploads and downloads,
// enforces ownership using JWT claims, and stores metadata in the database. Files never pass through the API server, which 
// keeps uploads scalable and secure

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
    // --------------------------------
    // DTO Mapping
    // --------------------------------
    // Converts Attachment entity → AttachmentDto
    // Keeps API responses stable and avoids leaking EF entities
    // SEARCH TERM: ATTACHMENTS_TO_DTO
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

    // --------------------------------
    // Request / Response Contracts
    // --------------------------------
    // Explicit request/response types keep endpoints self-documenting

    // SEARCH TERM: ATTACHMENTS_PRESIGN_UPLOAD_REQUEST
    public record PresignUploadRequest(
        string FileName,
        string ContentType,
        long SizeBytes
    );

    // SEARCH TERM: ATTACHMENTS_PRESIGN_UPLOAD_RESPONSE
    public record PresignUploadResponse(
        string UploadUrl,
        string StorageKey,
        int ExpiresInSeconds
    );

    // SEARCH TERM: ATTACHMENTS_PRESIGN_DOWNLOAD_RESPONSE
    public record PresignDownloadResponse(
        string DownloadUrl,
        int ExpiresInSeconds
    );

    // SEARCH TERM: ATTACHMENTS_CREATE_ATTACHMENT_REQUEST
    public record CreateAttachmentRequest(
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey
    );

    public static void MapAttachments(this WebApplication app)
    {
        // --------------------------------
        // Route Group Setup
        // --------------------------------
        // All attachment endpoints:
        // - Live under /api
        // - Require authentication by default
        // SEARCH TERM: ATTACHMENTS_GROUP_SETUP
        var group = app.MapGroup("/api")
            .RequireAuthorization();

        // ================================
        // PRESIGN UPLOAD
        // ================================
        // POST /api/job-apps/{jobAppId}/attachments/presign-upload
        // - Validates ownership + file constraints
        // - Generates a presigned PUT URL
        // - Frontend uploads directly to S3 (API not in data path)
        // SEARCH TERM: ATTACHMENTS_PRESIGN_UPLOAD_ENDPOINT
        group.MapPost("/job-apps/{jobAppId:int}/attachments/presign-upload", async (
            int jobAppId,
            PresignUploadRequest req,
            AppDbContext db,
            ClaimsPrincipal user,
            IAmazonS3 s3) =>
        {
            const long MaxFileSizeBytes = 25L * 1024L * 1024L; // 25 MB limit
            const int ExpSeconds = 10 * 60; // URL valid for 10 minutes

            // Basic request validation
            if (string.IsNullOrWhiteSpace(req.FileName))
                return Results.BadRequest(new { error = "FileName is required." });

            if (req.SizeBytes <= 0)
                return Results.BadRequest(new { error = "SizeBytes must be > 0." });

            if (req.SizeBytes > MaxFileSizeBytes)
                return Results.BadRequest(new { error = "File too large (max 25 MB)." });

            // Extract user identity from JWT
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Authorization guard: ensure job app belongs to this user
            var ownsApp = await db.JobApplications
                .AnyAsync(x => x.Id == jobAppId && x.UserId == userId);

            if (!ownsApp) return Results.NotFound();

            // Resolve S3 bucket from environment
            var bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
            if (string.IsNullOrWhiteSpace(bucket))
                return Results.Problem("S3_BUCKET_NAME is not set.");

            // Build a safe, scoped storage key
            // SEARCH TERM: ATTACHMENTS_S3_KEY_FORMAT
            var safeFileName = Path.GetFileName(req.FileName.Trim());
            var guid = Guid.NewGuid().ToString("N");

            // User-scoped + job-scoped path simplifies authorization and cleanup
            var storageKey =
                $"users/{userId}/job-apps/{jobAppId}/attachments/{guid}/{safeFileName}";

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
        // LIST ATTACHMENTS FOR JOB APP
        // ================================
        // GET /api/job-apps/{jobAppId}/attachments
        // - Returns metadata only (no file data)
        // SEARCH TERM: ATTACHMENTS_LIST_FOR_JOBAPP
        group.MapGet("/job-apps/{jobAppId:int}/attachments", async (
            int jobAppId,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Ownership check
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
        // CREATE ATTACHMENT METADATA
        // ================================
        // POST /api/job-apps/{jobAppId}/attachments
        // - Called AFTER successful S3 upload
        // - Persists metadata only
        // SEARCH TERM: ATTACHMENTS_CREATE_METADATA_ONLY
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
                return Results.BadRequest(new { error = "StorageKey is required." });

            // Guardrail: storage key must belong to this user + job app
            // SEARCH TERM: ATTACHMENTS_STORAGEKEY_OWNERSHIP_CHECK
            var expectedPrefix =
                $"users/{userId}/job-apps/{jobAppId}/attachments/";

            if (!req.StorageKey.StartsWith(expectedPrefix, StringComparison.Ordinal))
                return Results.BadRequest(new { error = "Invalid StorageKey." });

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

            return Results.Created(
                $"/api/attachments/{attachment.Id}",
                ToDto(attachment)
            );
        });

        // ================================
        // PRESIGN DOWNLOAD
        // ================================
        // GET /api/attachments/{id}/presign-download
        // - Returns temporary GET URL for private S3 object
        // SEARCH TERM: ATTACHMENTS_PRESIGN_DOWNLOAD_ENDPOINT
        group.MapGet("/attachments/{id:int}/presign-download", async (
            int id,
            AppDbContext db,
            ClaimsPrincipal user,
            IAmazonS3 s3) =>
        {
            const int ExpSeconds = 10 * 60;

            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var attachment = await db.Attachments
                .Include(a => a.JobApplication!)
                .FirstOrDefaultAsync(a =>
                    a.Id == id && a.JobApplication!.UserId == userId);

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
        // DELETE ATTACHMENT
        // ================================
        // DELETE /api/attachments/{id}
        // - Deletes DB metadata
        // - Best-effort deletes S3 object
        // SEARCH TERM: ATTACHMENTS_DELETE_ENDPOINT_WITH_S3_DELETE
        group.MapDelete("/attachments/{id:int}", async (
            int id,
            AppDbContext db,
            ClaimsPrincipal user,
            IAmazonS3 s3) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var attachment = await db.Attachments
                .Include(a => a.JobApplication!)
                .FirstOrDefaultAsync(a =>
                    a.Id == id && a.JobApplication!.UserId == userId);

            if (attachment is null) return Results.NotFound();

            var bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
            if (!string.IsNullOrWhiteSpace(bucket)
                && !string.IsNullOrWhiteSpace(attachment.StorageKey))
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
                    // Best-effort cleanup: metadata is still removed
                }
            }

            db.Attachments.Remove(attachment);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
