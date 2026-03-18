using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Business.Services.Datasets;

public interface IDatasetService
{
    Task<PagedResponse<DatasetResponse>> GetDatasets(DatasetQueryParameters @params);
    Task<PagedResponse<DatasetResponse>> GetProjectDatasets(Guid projectId, DatasetQueryParameters @params);
    Task<DatasetResponse?> GetDatasetById(Guid id);
    Task<DatasetResponse> CreateDataset(CreateDatasetRequest request);
    Task<DatasetResponse> UpdateDataset(Guid id, UpdateDatasetRequest request);
    Task DeleteDataset(Guid id);
    Task AddDatasetToProject(Guid datasetId, Guid projectId);
    Task RemoveDatasetFromProject(Guid datasetId, Guid projectId);
}
