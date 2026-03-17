using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Tasks;

public class LabelingTaskService : ILabelingTaskService
{
    private readonly ILabelingTaskRepository _taskRepo;
    private readonly IAssignmentRepository _assignmentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectMemberRepository _projectMemberRepo;
    private readonly IDatasetItemRepository _datasetItemRepo;
    private readonly ILabelingTaskItemRepository _taskItemRepo;

    public LabelingTaskService(
        ILabelingTaskRepository taskRepo,
        IAssignmentRepository assignmentRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IProjectRepository projectRepo,
        IProjectMemberRepository projectMemberRepo,
        IDatasetItemRepository datasetItemRepo,
        ILabelingTaskItemRepository taskItemRepo)
    {
        _taskRepo = taskRepo;
        _assignmentRepo = assignmentRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _projectRepo = projectRepo;
        _projectMemberRepo = projectMemberRepo;
        _datasetItemRepo = datasetItemRepo;
        _taskItemRepo = taskItemRepo;
    }

    public async Task<PagedResult<TaskAssignmentInfo>> GetTasksAsync(
        Guid userId, string userRole, TaskQueryParameters @params)
    {
        var now = DateTime.UtcNow;

        List<Assignment> assignments;

        // Role-based assignment loading
        if (userRole == "reviewer" || userRole == "annotator")
            assignments = await _assignmentRepo.GetByAssignedToAsync(userId);
        else
            assignments = await _assignmentRepo.GetAllAsync();

        // Filter assignments first
        var filteredAssignments = assignments
            .Where(a => a.StartedAt.HasValue)
            .Select(a => new
            {
                Assignment = a,
                DeadlineAt = a.StartedAt!.Value.AddMinutes(a.TimeLimitMinutes)
            })
            .Where(x =>
            {
                if (@params.IsExpired == null || @params.IsExpired == false)
                    return now < x.DeadlineAt;
                else
                    return now > x.DeadlineAt;
            })
            .ToList();

        if (!filteredAssignments.Any())
        {
            return new PagedResult<TaskAssignmentInfo>
            {
                Items = new List<TaskAssignmentInfo>(),
                Page = @params.Page,
                PageSize = @params.PageSize,
                TotalItems = 0,
                TotalPages = 0
            };
        }

        // Batch load tasks (same efficiency as function 1)
        var taskIds = filteredAssignments
            .Select(x => x.Assignment.TaskId)
            .Distinct()
            .ToList();

        var tasks = await _taskRepo.GetByIdsAsync(taskIds);

        // Optional status filtering
        if (@params.Status.HasValue)
            tasks = tasks.Where(t => t.Status == @params.Status.Value).ToList();

        if (@params.ProjectId.HasValue)
            tasks = tasks.Where(t => t.ProjectId == @params.ProjectId).ToList();

        var taskDict = tasks.ToDictionary(t => t.TaskId);

        var results = new List<TaskAssignmentInfo>();

        foreach (var item in filteredAssignments)
        {
            if (!taskDict.TryGetValue(item.Assignment.TaskId, out var task))
                continue;

            results.Add(new TaskAssignmentInfo
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Status = task.Status.ToString(),
                AssignedTo = item.Assignment.AssignedTo,
                AssignedBy = item.Assignment.AssignedBy,
                AssignedAt = item.Assignment.AssignedAt,
                DeadlineAt = item.DeadlineAt
            });
        }

        // Sort by deadline DESC
        results = results
            .OrderByDescending(x => x.DeadlineAt)
            .ToList();

        // Pagination
        var totalCount = results.Count;

        var paginatedItems = results
            .Skip((@params.Page - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .ToList();

        return new PagedResult<TaskAssignmentInfo>
        {
            Items = paginatedItems,
            Page = @params.Page,
            PageSize = @params.PageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)@params.PageSize)
        };
    }

    public async Task<LabelingTask?> GetTaskByIdForUserAsync(Guid taskId, Guid userId, string userRole)
    {
        if (userRole == "admin" && userRole == "manager")
        {
            return await _taskRepo.GetByIdAsync(taskId);
        }

        var assignment = await _assignmentRepo.GetByTaskIdAndUserAsync(taskId, userId);
        if (assignment == null)
            return null;

        if (!assignment.StartedAt.HasValue)
            return null;

        var now = DateTime.UtcNow;
        var deadline = assignment.StartedAt.Value.AddMinutes(assignment.TimeLimitMinutes);

        // Ignore expired assignments
        if (now >= deadline)
            return null;

        // Fetch task only after assignment validation
        var task = await _taskRepo.GetByIdAsync(taskId);
        return task;
    }

    public async Task<List<Assignment>> UpdateAssignmentsByDatasetAsync(
        Guid assignmentId, Guid datasetId, double timeLimitMinutes)
    {
        if (timeLimitMinutes <= 0)
            throw new Exception("Time limit must be positive");

        // Get the assignment
        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new Exception("Assignment not found");

        // Cannot update if assignment is already finished
        // if (assignment.Status == AssignmentStatus.Expired || assignment.Status == AssignmentStatus.Completed)
        //     throw new Exception("Cannot update time limit: assignment is already finished");

        // Get dataset items
        var datasetItems = await _datasetItemRepo.GetAllByDatasetIdAsync(datasetId);
        var datasetItemIds = datasetItems.Select(di => di.DatasetItemId).ToList();

        if (datasetItemIds.Count == 0)
            throw new Exception("No dataset items found for this dataset");

        // Get tasks by dataset item IDs
        var tasks = await _taskRepo.GetByDatasetItemIdsAsync(datasetItemIds);
        var taskIds = tasks.Select(t => t.TaskId).ToList();

        // Get assignments for these tasks and the same user
        var allAssignments = await _assignmentRepo.GetByAssignedToAsync(assignment.AssignedTo);
        var assignmentsToUpdate = allAssignments
            .Where(a => taskIds.Contains(a.TaskId))
            .ToList();

        if (assignmentsToUpdate.Count == 0)
            throw new Exception("No assignments found for the specified dataset");

        // Cannot update finished assignments
        // if (assignmentsToUpdate.Any(a => a.Status == AssignmentStatus.Expired || a.Status == AssignmentStatus.Completed))
        //     throw new Exception("Cannot update time limit: some assignments are already finished");

        // Update time limits
        foreach (var a in assignmentsToUpdate)
        {
            a.TimeLimitMinutes = timeLimitMinutes;
        }

        await _assignmentRepo.UpdateRangeAsync(assignmentsToUpdate);
        await _assignmentRepo.SaveChangesAsync();

        return assignmentsToUpdate;
    }

    public async Task<List<LabelingTaskItem>> AssignTaskItemsToTaskAsync(Guid taskId, IEnumerable<Guid> taskItemIds)
    {
        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null)
            throw new Exception("Task not found");

        var taskItems = await _taskItemRepo.GetByIdsAsync(taskItemIds);
        if (taskItems == null || taskItems.Count == 0)
            throw new Exception("No task items found");

        // Ensure all task items belong to the same project as the task
        if (taskItems.Any(ti => ti.ProjectId != task.ProjectId))
            throw new Exception("All task items must belong to the same project as the task");

        foreach (var taskItem in taskItems)
        {
            taskItem.TaskId = taskId;
            taskItem.Status = LabelingTaskItemStatus.Assigned;
        }

        await _taskItemRepo.UpdateRangeAsync(taskItems);
        await _taskItemRepo.SaveChangesAsync();

        return taskItems;
    }

    public async Task<List<LabelingTaskItem>> GetTaskItemsByProjectIdAsync(Guid projectId)
    {
        return await _taskItemRepo.GetByProjectIdAsync(projectId);
    }
}
