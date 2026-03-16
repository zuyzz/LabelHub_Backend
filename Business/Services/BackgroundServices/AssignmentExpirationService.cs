using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataLabelProject.Business.Services.BackgroundServices;

public class AssignmentExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AssignmentExpirationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every 1 minute

    public AssignmentExpirationService(
        IServiceProvider serviceProvider,
        ILogger<AssignmentExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Assignment Expiration Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireAssignmentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking assignment expirations");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Assignment Expiration Service is stopping");
    }

    private async Task CheckAndExpireAssignmentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var assignmentRepo = scope.ServiceProvider.GetRequiredService<IAssignmentRepository>();
        var taskRepo = scope.ServiceProvider.GetRequiredService<ILabelingTaskRepository>();
        var taskItemRepo = scope.ServiceProvider.GetRequiredService<ILabelingTaskItemRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var expiredAssignments = await assignmentRepo.GetExpiredAsync();

        foreach (var assignment in expiredAssignments)
        {
            // Get user to check role
            var user = await userRepo.GetByIdAsync(assignment.AssignedTo);
            if (user == null)
                continue;

            var role = await roleRepo.GetByIdAsync(user.RoleId);
            if (role == null || role.RoleName != "reviewer")
                continue; // Only expire reviewer assignments

            // Get task
            var task = await taskRepo.GetByIdAsync(assignment.TaskId);
            if (task == null || task.Status == LabelingTaskStatus.Closed)
                continue;

            // Close task
            task.Status = LabelingTaskStatus.Closed;

            // Get task items
            var taskItems = await taskItemRepo.GetByTaskIdAsync(assignment.TaskId);

            // Mark assigned items as incompleted
            foreach (var item in taskItems)
            {
                if (item.Status == LabelingTaskItemStatus.Assigned)
                {
                    item.Status = LabelingTaskItemStatus.Incompleted;
                }
            }

            // Save changes
            await taskItemRepo.UpdateRangeAsync(taskItems);
            await taskItemRepo.SaveChangesAsync();
            await taskRepo.SaveChangesAsync();

            _logger.LogInformation($"[Assignment Expiration] Closed task {task.TaskId} for expired reviewer assignment {assignment.AssignmentId}");
        }
    }
}
