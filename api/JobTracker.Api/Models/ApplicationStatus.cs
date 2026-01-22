namespace JobTracker.Api.Models;

/**
 * ApplicationStatus (Workflow Enum)
 * - Purpose: Defines valid workflow states for a JobApplication.
 * - Interview talking point: Using an enum prevents invalid strings and maps cleanly to Kanban columns.
 */
public enum ApplicationStatus
{
    Draft = 0,
    Applied = 1,
    Interviewing = 2,
    Offer = 3,
    Rejected = 4,
    Accepted = 5,
}
