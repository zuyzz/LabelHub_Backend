using Microsoft.AspNetCore.Http;
using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Services;

public interface IDatasetService
{
    Task<DatasetImportResponse> ImportDatasetAsync(Guid projectId, DatasetImportRequest request);
}