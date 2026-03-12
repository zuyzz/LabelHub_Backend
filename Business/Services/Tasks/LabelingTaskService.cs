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

    public LabelingTaskService(
        ILabelingTaskRepository taskRepo,
        IAssignmentRepository assignmentRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IProjectRepository projectRepo,
        IProjectMemberRepository projectMemberRepo)
    {
        _taskRepo = taskRepo;
        _assignmentRepo = assignmentRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _projectRepo = projectRepo;
        _projectMemberRepo = projectMemberRepo;
    }

    public async Task<List<LabelingTask>> GetTasksForUserAsync(Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole == "admin" || currentUserRole == "manager")
            return await _taskRepo.GetAllAsync();

        // reviewer or annotator: active tasks assigned to them (not expired), sorted DESC
        var assignments = await _assignmentRepo.GetByAssignedToAsync(currentUserId);

        var now = DateTime.UtcNow;
        var taskIds = assignments
            .Where(a => a.StartedAt.HasValue && now < a.StartedAt.Value.AddMinutes(a.TimeLimitMinutes))
            .OrderByDescending(a => a.StartedAt.HasValue ? a.StartedAt.Value.AddMinutes(a.TimeLimitMinutes) : DateTime.MinValue)
            .Select(a => a.TaskId)
            .Distinct()
            .ToList();

        var tasks = await _taskRepo.GetByIdsAsync(taskIds);

        // Preserve deadline DESC sort order (EF IN query does not guarantee order)
        return taskIds
            .Select(id => tasks.First(t => t.TaskId == id))
            .ToList();
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
        // Validate project exists and is active
        var project = await _projectRepo.GetByIdAsync(projectId);
        if (project == null)
            throw new Exception("Project not found");
        if (!project.IsActive)
            throw new Exception("Project is not active");

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
        var existedMember = await _projectMemberRepo.GetByIdAsync(projectId, assignedTo);
        if (existedMember == null)
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
            StartedAt = DateTime.UtcNow,
            TimeLimitMinutes = 7 * 24 * 60,                
            Status = AssignmentStatus.incompleted
        };

        await _assignmentRepo.AddAsync(assignment);
        await _assignmentRepo.SaveChangesAsync();

        return assignment;
    }

    public async Task<Assignment> UpdateTimeLimitAsync(Guid taskId, double timeLimitMinutes)
    {
        if (timeLimitMinutes <= 0)
            throw new Exception("Time limit must be positive");

        var assignment = await _assignmentRepo.GetByTaskIdAsync(taskId);
        if (assignment == null)
            throw new Exception("Assignment not found for this task");

        assignment.TimeLimitMinutes = timeLimitMinutes;
        await _assignmentRepo.UpdateAsync(assignment);
        await _assignmentRepo.SaveChangesAsync();

        return assignment;
    }
}