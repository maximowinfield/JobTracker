namespace JobTracker.Api.Dtos;

public record AttachmentDto(
    int Id,
    int JobApplicationId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    DateTime CreatedAtUtc
);
