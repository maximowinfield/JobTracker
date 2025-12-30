namespace JobTracker.Api.Models;

public enum ApplicationStatus
{
    Draft = 0,
    Applied = 1,
    Interviewing = 2,
    Offer = 3,
    Rejected = 4,
    Accepted = 5,
}
