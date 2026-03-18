using DataLabelProject.Application.DTOs.Tasks;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly ILabelingTaskRepository _taskRepo;
    private readonly ILabelingTaskItemRepository _taskItemRepo;
    private readonly IAssignmentRepository _assignmentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectMemberRepository _projectMemberRepo;
    private readonly IDatasetRepository _datasetRepo;
    private readonly IDatasetItemRepository _datasetItemRepo;
    private readonly ICurrentUserService _currentUserService;

    public AssignmentService(
        ILabelingTaskRepository taskRepo,
        ILabelingTaskItemRepository taskItemRepo,
        IAssignmentRepository assignmentRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IProjectRepository projectRepo,
        IProjectMemberRepository projectMemberRepo,
        IDatasetRepository datasetRepo,
        IDatasetItemRepository datasetItemRepo,
        ICurrentUserService currentUserService)
    {
        _taskRepo = taskRepo;
        _taskItemRepo = taskItemRepo;
        _assignmentRepo = assignmentRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _projectRepo = projectRepo;
        _projectMemberRepo = projectMemberRepo;
        _datasetRepo = datasetRepo;
        _datasetItemRepo = datasetItemRepo;
        _currentUserService = currentUserService;
    }

    public async Task<TaskAssignmentResponse> AssignTaskAsync(BulkAssignTaskRequest request, Guid assignedBy)
    {
        // Get dataset items
        var datasetItems = await _datasetItemRepo.GetAllByDatasetIdAsync(request.DatasetId);
        var datasetItemIds = datasetItems.Select(di => di.DatasetItemId).ToList();

        if (datasetItemIds.Count == 0)
            throw new Exception("No dataset items found for this dataset");

        // Get unassigned task items
        var unassignedTaskItems = await _taskItemRepo.GetUnassignedByDatasetItemIdsAsync(datasetItemIds);

        if (unassignedTaskItems.Count == 0)
            throw new Exception("No unassigned task items found for this dataset");

        // Validate user exists
        var user = await _userRepo.GetByIdAsync(request.AssignedTo);
        if (user == null)
            throw new Exception("User not found");

        // Validate project exists
        var project = await _projectRepo.GetByIdAsync(request.ProjectId);
        if (project == null)
            throw new Exception("Project not found");

        // Validate dataset belongs to project
        var dataset = await _datasetRepo.GetByIdAsync(request.DatasetId);
        if (dataset == null)
            throw new Exception("Dataset not found");
        if (dataset.ProjectId != request.ProjectId)
            throw new Exception("Dataset does not belong to the specified project");

        // Validate user is member of project
        var isMember = await _projectMemberRepo.GetByIdAsync(request.ProjectId, request.AssignedTo);
        if (isMember == null)
            throw new Exception("User is not a member of this project");

        // Validate user role is reviewer or annotator
        var role = await _roleRepo.GetByIdAsync(user.RoleId);
        if (role == null || (role.RoleName != "reviewer" && role.RoleName != "annotator"))
            throw new Exception("User must have role 'reviewer' or 'annotator'");

        // Create new task
        var newTask = new LabelingTask
        {
            TaskId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            Status = LabelingTaskStatus.Opened
        };

        await _taskRepo.AddAsync(newTask);
        await _taskRepo.SaveChangesAsync();

        // Link unassigned task items to task
        foreach (var item in unassignedTaskItems)
        {
            item.TaskId = newTask.TaskId;
            item.Status = LabelingTaskItemStatus.Assigned;
        }

        await _taskItemRepo.UpdateRangeAsync(unassignedTaskItems);
        await _taskItemRepo.SaveChangesAsync();

        DateTime? startedAt = null;
        if (role != null && role.RoleName == "annotator")
        {
            startedAt = DateTime.UtcNow;
        }

        // Create assignment (starts immediately)
        var assignment = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            TaskId = newTask.TaskId,
            AssignedTo = request.AssignedTo,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            StartedAt = request.StartedAt,
            DeadlineAt = request.DeadlineAt
        };

        await _assignmentRepo.AddAsync(assignment);
        await _assignmentRepo.SaveChangesAsync();

        return new TaskAssignmentResponse
        {
            TaskId = newTask.TaskId,
            ProjectId = newTask.ProjectId,
            AssignedTo = assignment.AssignedTo,
            AssignedBy = assignment.AssignedBy,
            AssignedAt = assignment.AssignedAt,
            DeadlineAt = assignment.DeadlineAt
        };
    }
}
