namespace DataLabelProject.Application.DTOs.Labels
{
    public class CreateLabelRequest
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public Guid CreatedBy { get; set; }
    }
}
