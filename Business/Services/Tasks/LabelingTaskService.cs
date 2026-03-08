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

    public LabelingTaskService(
        ILabelingTaskRepository taskRepo,
        IAssignmentRepository assignmentRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IProjectRepository projectRepo)
    {
        _taskRepo = taskRepo;
        _assignmentRepo = assignmentRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _projectRepo = projectRepo;
    }

    public async Task<List<LabelingTask>> GetTasksForUserAsync(Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole == "admin" || currentUserRole == "manager")
        {
            return await _taskRepo.GetAllAsync();
        }

        // reviewer or annotator: get tasks assigned to current user where deadline has not passed
        var assignments = await _assignmentRepo.GetByAssignedToAsync(currentUserId);

        var taskIds = assignments
            .Where(a => DateTime.UtcNow < a.DeadlineAt)
            .OrderByDescending(a => a.DeadlineAt)
            .Select(a => a.TaskId)
            .ToList();

        return await _taskRepo.GetByIdsAsync(taskIds);
    }

    public async Task<LabelingTask> CreateTaskAsync(Guid datasetItemId, Guid projectId)
    {
        var task = new LabelingTask
        {
            TaskId = Guid.NewGuid(),
            DatasetItemId = datasetItemId,
            ProjectId = projectId
        };

        await _taskRepo.AddAsync(task);
        await _taskRepo.SaveChangesAsync();

        return task;
    }

    public async Task<Assignment> AssignTaskAsync(Guid taskId, Guid projectId, Guid assignedTo, Guid assignedBy)
    {
        // Validate task exists and belongs to the given project
        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null)
            throw new Exception("Task not found");

        if (task.ProjectId != projectId)
            throw new Exception("Task does not belong to the specified project");

        // Validate assignee exists
        var user = await _userRepo.GetByIdAsync(assignedTo);
        if (user == null)
            throw new Exception("User not found");

        // Validate assignee is a member of the project
        var isMember = await _projectRepo.ProjectMemberExistsAsync(projectId, assignedTo);
        if (!isMember)
            throw new Exception("User is not a member of this project");

        // Validate assignee role is reviewer or annotator
        var role = await _roleRepo.GetByIdAsync(user.RoleId);
        if (role == null || (role.RoleName != "reviewer" && role.RoleName != "annotator"))
            throw new Exception("User must have role 'reviewer' or 'annotator' to be assigned a task");

        var assignment = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            TaskId = taskId,
            AssignedTo = assignedTo,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            DeadlineAt = DateTime.UtcNow.AddDays(7),
            Status = AssignmentStatus.incompleted
        };

        await _assignmentRepo.AddAsync(assignment);
        await _assignmentRepo.SaveChangesAsync();

        return assignment;
    }

    public async Task<Assignment> UpdateDeadlineAsync(Guid taskId, DateTime deadlineAt)
    {
        if (deadlineAt <= DateTime.UtcNow)
            throw new Exception("Deadline must be in the future");

        var assignment = await _assignmentRepo.GetByTaskIdAsync(taskId);
        if (assignment == null)
            throw new Exception("Assignment not found for this task");

        assignment.DeadlineAt = deadlineAt;
        await _assignmentRepo.UpdateAsync(assignment);
        await _assignmentRepo.SaveChangesAsync();

        return assignment;
    }
}
