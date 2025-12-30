namespace JobTracker.Api.Models;

public class JobApplication
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public string Company { get; set; } = "";
    public string RoleTitle { get; set; } = "";

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Attachment> Attachments { get; set; } = new();
}
