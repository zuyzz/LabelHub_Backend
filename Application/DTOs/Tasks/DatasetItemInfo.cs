namespace DataLabelProject.Application.DTOs.Tasks;

public class DatasetItemInfo
{
    public Guid ItemId { get; set; }
    public string StorageUri { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
}
