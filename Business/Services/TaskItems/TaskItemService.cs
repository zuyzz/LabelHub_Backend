using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Services.Assignments;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.TaskItems;

public class TaskItemService : ITaskItemService
{
    private readonly ILabelingTaskRepository _taskRepo;
    private readonly ILabelingTaskItemRepository _taskItemRepo;
    private readonly IAssignmentRepository _assignmentRepo;
    private readonly IAssignmentService _assignmentService;

    public TaskItemService(
        ILabelingTaskRepository taskRepo,
        ILabelingTaskItemRepository taskItemRepo,
        IAssignmentRepository assignmentRepo,
        IAssignmentService assignmentService)
    {
        _taskRepo = taskRepo;
        _taskItemRepo = taskItemRepo;
        _assignmentRepo = assignmentRepo;
        _assignmentService = assignmentService;
    }

    public async Task<PagedResult<TaskItemDetailResponse>> GetTaskItemsByTaskAsync(Guid taskId, TaskItemQueryParameters @params)
    {
        // Validate task exists
        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null)
            throw new Exception("Task not found");

        // Get task items
        var taskItems = await _taskItemRepo.GetByTaskIdAsync(taskId);

        // Get assignment to compute deadline
        var assignment = await _assignmentRepo.GetByTaskIdAsync(taskId);
        if (assignment == null)
            throw new Exception("Assignment not found for this task");

        var now = DateTime.UtcNow;
        DateTime? deadlineAt = null;

        if (assignment.StartedAt.HasValue)
        {
            deadlineAt = _assignmentService.ComputeDeadline(assignment);
        }

        var result = new List<TaskItemDetailResponse>();

        foreach (var item in taskItems)
        {
            // Filter by expiration status
            // Spec: IsExpired == null or false → exclude expired (skip if now >= deadlineAt)
            //       IsExpired == true → only expired (skip if now <= deadlineAt)
            if (@params.IsExpired == null || @params.IsExpired.Value == false)
            {
                if (!deadlineAt.HasValue)
                    continue; // Skip if no deadline (can't determine expiration)

                if (now >= deadlineAt.Value)
                    continue;
            }
            else // IsExpired == true
            {
                if (!deadlineAt.HasValue)
                    continue; // Skip if no deadline

                if (now <= deadlineAt.Value)
                    continue;
            }

            // Filter by status
            if (@params.Status.HasValue && item.Status != @params.Status.Value)
                continue;

            result.Add(new TaskItemDetailResponse
            {
                TaskItemId = item.TaskItemId,
                ProjectId = item.ProjectId,
                DatasetItem = new DatasetItemInfo
                {
                    ItemId = item.DatasetItem.DatasetItemId,
                    StorageUri = item.DatasetItem.StorageUri,
                    Metadata = item.DatasetItem.Metadata
                },
                RevisionCount = item.RevisionCount,
                Status = item.Status.ToString()
            });
        }

        // Pagination
        var totalCount = result.Count;
        var paginatedItems = result
            .Skip((@params.Page - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .ToList();

        return new PagedResult<TaskItemDetailResponse>
        {
            Items = paginatedItems,
            Page = @params.Page,
            PageSize = @params.PageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)@params.PageSize)
        };
    }
}
