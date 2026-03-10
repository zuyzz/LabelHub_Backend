using DataLabelProject.Application.DTOs.Exports;

namespace DataLabelProject.Business.Services.Exports;

public interface IExportService
{
    Task<IEnumerable<ExportJobResponse>> GetExports();
    Task<ExportJobResponse?> GetExportById(Guid exportId);
    Task<ExportJobResponse> CreateExport(Guid projectId, CreateExportRequest request);
}
