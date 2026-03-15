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
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1); // Check every 1 second

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

        var allAssignments = await assignmentRepo.GetAllAsync();
        var now = DateTime.UtcNow;

        // Filter only assignments that need to be checked (started and not already expired/completed)
        var activeAssignments = allAssignments
            .Where(a => a.StartedAt.HasValue && a.Status == AssignmentStatus.Incompleted)
            .ToList();

        var expiredAssignments = new List<Business.Models.Assignment>();

        foreach (var assignment in activeAssignments)
        {
            var deadline = assignment.StartedAt!.Value.AddMinutes(assignment.TimeLimitMinutes);
            if (now > deadline)
            {
                // assignment.Status = AssignmentStatus.Expired;
                expiredAssignments.Add(assignment);
            }
        }

        if (expiredAssignments.Count > 0)
        {
            await assignmentRepo.UpdateRangeAsync(expiredAssignments);
            await assignmentRepo.SaveChangesAsync();
            _logger.LogInformation($"[Assignment Expiration] Expired {expiredAssignments.Count} assignment(s)");
        }
    }
}
