using System;
using System.Threading.Tasks;

namespace DataLabelProject.Business.Services.Export;

public interface IExportService
{
    /// <summary>
    /// Exports a dataset as COCO format JSON.
    /// Returns a stream containing the COCO JSON data.
    /// </summary>
    Task<Stream> ExportAsCocoJsonAsync(Guid datasetId);
}
