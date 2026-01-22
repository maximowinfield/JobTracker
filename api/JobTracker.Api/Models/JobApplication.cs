namespace JobTracker.Api.Models;

/**
 * JobApplication (Domain Entity)
 * - Purpose: Represents one tracked application in the JobTracker workflow.
 * - Interview talking point: Status maps directly to Kanban columns; ownership is enforced via UserId.
 */
public class JobApplication
{
    // Primary key (Entity Framework Core convention: "Id" is the key)
    public int Id { get; set; }

    // Foreign key: the authenticated owner of this job application
    public int UserId { get; set; }

    // Optional navigation property (only if you model Users as an entity in this version)
    public AppUser? User { get; set; }

    // Required fields (non-nullable strings initialized to empty to avoid null checks)
    public string Company { get; set; } = "";
    public string RoleTitle { get; set; } = "";

    // Workflow state (enum) used by both API + Kanban UI
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

    // Optional free-form notes
    public string? Notes { get; set; }

    // UTC timestamps help consistency across time zones / deployments
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Optional relationship (only if attachments are modeled as a separate entity in this version)
    public List<Attachment> Attachments { get; set; } = new();
}
