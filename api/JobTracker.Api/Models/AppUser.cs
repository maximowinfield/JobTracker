namespace JobTracker.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<JobApplication> JobApplications { get; set; } = new();
}
