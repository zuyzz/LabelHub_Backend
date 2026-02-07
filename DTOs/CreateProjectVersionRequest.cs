namespace DataLabel_Project_BE.DTOs
{
    public class CreateProjectVersionRequest
    {
        public Guid DatasetId { get; set; }
        public Guid LabelSetId { get; set; }
        public Guid GuidelineId { get; set; }
    }
}
