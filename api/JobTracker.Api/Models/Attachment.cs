namespace JobTracker.Api.Models;

public class Attachment
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    // metadata only for now; actual file will live in S3 later
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }

    // S3 object key later (e.g., "users/12/apps/99/resume.pdf")
    public string StorageKey { get; set; } = "";

    public DateTime? DeletedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
