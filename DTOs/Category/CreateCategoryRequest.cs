namespace DataLabel_Project_BE.DTOs.Category
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
