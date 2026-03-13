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

    public LabelingTaskService(
        ILabelingTaskRepository taskRepo,
        IAssignmentRepository assignmentRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IProjectRepository projectRepo,
        IProjectMemberRepository projectMemberRepo,
        IDatasetItemRepository datasetItemRepo)
    {
        _taskRepo = taskRepo;
        _assignmentRepo = assignmentRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _projectRepo = projectRepo;
        _projectMemberRepo = projectMemberRepo;
        _datasetItemRepo = datasetItemRepo;
    }

    public async Task<(List<LabelingTask> Tasks, int TotalCount)> GetTasksForReviewerAsync(
        Guid reviewerId, LabelingTaskStatus? status, int page, int pageSize)
    {
        return await GetTasksForUserWithRoleAsync(reviewerId, status, page, pageSize);
    }

    public async Task<(List<LabelingTask> Tasks, int TotalCount)> GetTasksForAnnotatorAsync(
        Guid annotatorId, LabelingTaskStatus? status, int page, int pageSize)
    {
        return await GetTasksForUserWithRoleAsync(annotatorId, status, page, pageSize);
    }

    private async Task<(List<LabelingTask> Tasks, int TotalCount)> GetTasksForUserWithRoleAsync(
        Guid userId, LabelingTaskStatus? status, int page, int pageSize)
    {
        var assignments = await _assignmentRepo.GetByAssignedToAsync(userId);
        var now = DateTime.UtcNow;

        // Filter: StartedAt != null AND currentTime < deadline
        var activeAssignments = assignments
            .Where(a => a.StartedAt.HasValue && now < a.StartedAt.Value.AddMinutes(a.TimeLimitMinutes))
            .ToList();

        if (activeAssignments.Count == 0)
            return (new List<LabelingTask>(), 0);

        var taskIds = activeAssignments.Select(a => a.TaskId).Distinct().ToList();
        var tasks = await _taskRepo.GetByIdsAsync(taskIds);

        // Filter by status if provided
        if (status.HasValue)
        {
            tasks = tasks.Where(t => t.Status == status.Value).ToList();
        }

        // Sort by deadline DESC
        var tasksWithDeadline = tasks
            .Select(t => new
            {
                Task = t,
                Deadline = activeAssignments
                    .Where(a => a.TaskId == t.TaskId)
                    .Select(a => a.StartedAt!.Value.AddMinutes(a.TimeLimitMinutes))
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.Deadline)
            .ToList();

        var totalCount = tasksWithDeadline.Count;

        // Pagination
        var paginatedTasks = tasksWithDeadline
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Task)
            .ToList();

        return (paginatedTasks, totalCount);
    }

    public async Task<LabelingTask?> GetTaskByIdForUserAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null)
            return null;

        var assignment = await _assignmentRepo.GetByTaskIdAndUserAsync(taskId, userId);
        if (assignment == null)
            return null;

        // Check if assignment is still active
        if (!assignment.StartedAt.HasValue)
            return null;

        var now = DateTime.UtcNow;
        var deadline = assignment.StartedAt.Value.AddMinutes(assignment.TimeLimitMinutes);
        if (now >= deadline)
            return null;

        return task;
    }

    public async Task<List<Assignment>> BulkAssignTasksAsync(
        Guid datasetId, Guid projectId, Guid assignedTo, Guid assignedBy)
    {
        // Validate project exists and is active
        var project = await _projectRepo.GetByIdAsync(projectId);
        if (project == null)
            throw new Exception("Project not found");
        if (!project.IsActive)
            throw new Exception("Project is not active");

        // Validate assignee exists
        var user = await _userRepo.GetByIdAsync(assignedTo);
        if (user == null)
            throw new Exception("User not found");

        // Validate assignee is a member of the project
        var existedMember = await _projectMemberRepo.GetByIdAsync(projectId, assignedTo);
        if (existedMember == null)
            throw new Exception("User is not a member of this project");

        // Validate assignee role is reviewer or annotator
        var role = await _roleRepo.GetByIdAsync(user.RoleId);
        if (role == null || (role.RoleName != "reviewer" && role.RoleName != "annotator"))
            throw new Exception("User must have role 'reviewer' or 'annotator' to be assigned a task");

        // Get dataset items
        var datasetItems = await _datasetItemRepo.GetAllByDatasetIdAsync(datasetId);
        var datasetItemIds = datasetItems.Select(di => di.ItemId).ToList();

        if (datasetItemIds.Count == 0)
            throw new Exception("No dataset items found for this dataset");

        // Get tasks by dataset item IDs
        var tasks = await _taskRepo.GetByDatasetItemIdsAsync(datasetItemIds);

        if (tasks.Count == 0)
            throw new Exception("No tasks found for dataset items");

        // Validate all tasks belong to the project
        if (tasks.Any(t => t.ProjectId != projectId))
            throw new Exception("Some tasks do not belong to the specified project");

        // Create assignments (StartedAt = DateTime.UtcNow - tasks are immediately active)
        var assignments = tasks.Select(t => new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            TaskId = t.TaskId,
            AssignedTo = assignedTo,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
            TimeLimitMinutes = 7 * 24 * 60, // Default 7 days
            Status = AssignmentStatus.Incompleted
        }).ToList();

        await _assignmentRepo.AddRangeAsync(assignments);
        await _assignmentRepo.SaveChangesAsync();

        return assignments;
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

        // Check if task is already started
        if (assignment.StartedAt.HasValue && assignment.StartedAt.Value <= DateTime.UtcNow)
            throw new Exception("Cannot update time limit: the task is already started");

        // Get dataset items
        var datasetItems = await _datasetItemRepo.GetAllByDatasetIdAsync(datasetId);
        var datasetItemIds = datasetItems.Select(di => di.ItemId).ToList();

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

        // Check if any of them are already started
        if (assignmentsToUpdate.Any(a => a.StartedAt.HasValue && a.StartedAt.Value <= DateTime.UtcNow))
            throw new Exception("Cannot update time limit: some tasks are already started");

        // Update time limits
        foreach (var a in assignmentsToUpdate)
        {
            a.TimeLimitMinutes = timeLimitMinutes;
        }

        await _assignmentRepo.UpdateRangeAsync(assignmentsToUpdate);
        await _assignmentRepo.SaveChangesAsync();

        return assignmentsToUpdate;
    }
}