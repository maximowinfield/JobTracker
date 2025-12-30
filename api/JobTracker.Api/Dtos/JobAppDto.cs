using JobTracker.Api.Models;

namespace JobTracker.Api.Dtos;

public record JobAppDto(
    int Id,
    string Company,
    string RoleTitle,
    ApplicationStatus Status,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
