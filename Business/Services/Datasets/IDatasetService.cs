using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Business.Services.Datasets;

public interface IDatasetService
{
    Task<CreateDatasetResponse> CreateDatasetAsync(CreateDatasetRequest request);
    Task<DatasetResponse> GetDatasetByIdAsync(Guid datasetId);
    Task<UpdateDatasetResponse> UpdateDatasetAsync(Guid datasetId, UpdateDatasetRequest request);
    Task DeleteDatasetAsync(Guid datasetId);
}
