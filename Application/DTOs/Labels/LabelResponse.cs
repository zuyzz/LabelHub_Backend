using DataLabelProject.Application.DTOs.Categories;

namespace DataLabelProject.Application.DTOs.Labels
{
    public record LabelResponse
    {
        public Guid LabelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
        public CategoryResponse Category { get; set; } = null!;
    }
}
