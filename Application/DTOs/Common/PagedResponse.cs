namespace DataLabelProject.Application.DTOs.Common
{
    public record PagedResponse<T>
    {
        public IEnumerable<T> Items { get; init; } = [];
        public int TotalItems { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
