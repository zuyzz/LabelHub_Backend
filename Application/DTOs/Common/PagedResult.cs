namespace DataLabelProject.Application.DTOs.Common;

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalItems { get; init; }
}
