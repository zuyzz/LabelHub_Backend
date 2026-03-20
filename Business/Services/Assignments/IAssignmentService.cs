using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Assignments;

public interface IAssignmentService
{
    Task<BulkTaskAssignmentResponse> AssignTaskAsync(BulkAssignTaskRequest request, Guid assignedBy);
}
