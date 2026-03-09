using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Business.Services.Datasets;

public interface IDatasetService
{
    Task<PagedResponse<DatasetResponse>> GetDatasets(DatasetQueryParameters @params);
    Task<DatasetResponse?> GetDatasetById(Guid id);
    Task<DatasetResponse> CreateDataset(CreateDatasetRequest request);
    Task<DatasetResponse> UpdateDataset(Guid id, UpdateDatasetRequest request);
    Task<bool> DeleteDataset(Guid id);
    Task AddDatasetToProject(Guid datasetId, Guid projectId);
    Task RemoveDatasetFromProject(Guid datasetId, Guid projectId);
}
