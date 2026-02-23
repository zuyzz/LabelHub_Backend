using Microsoft.AspNetCore.Http;
using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Business.Services.Datasets;

public interface IDatasetService
{
    Task<DatasetImportResponse> ImportDatasetAsync(Guid projectId, DatasetImportRequest request);
}
