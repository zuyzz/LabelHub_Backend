namespace DataLabelProject.Application.DTOs.Projects
{
    public record ProjectCreateRequest(
        string Name,
        string? Description,
        Guid CategoryId
    );

    public record ProjectUpdateRequest(
        string Name,
        string? Description,
        string? Status,
        Guid CategoryId
    );

    public record ProjectResponse(
        Guid ProjectId,
        string Name,
        string? Description,
        string Status,
        Guid CategoryId,
        DateTime CreatedAt,
        Guid? CreatedBy
    );
}
