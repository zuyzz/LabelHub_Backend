using System.Text.Json;
using DataLabelProject.Application.DTOs.Statistics;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Data;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Business.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId)
    {
        return await _context.Projects.AnyAsync(p => p.ProjectId == projectId);
    }

    public async Task<bool> IsProjectMemberAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.MemberId == userId);
    }

    // =============== Project Statistics ===============

    public async Task<ProjectOverviewResponse> GetProjectOverviewAsync(Guid projectId)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Where(p => p.ProjectId == projectId)
            .Select(p => new { p.ProjectId, p.Name })
            .FirstAsync();

        var projectTaskItemIds = await GetProjectTaskItemIdsAsync(projectId);
        var datasetItems = projectTaskItemIds.Distinct().Count();

        // Get task counts by status
        var taskCounts = await _context.LabelingTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var annotationsTotal = await CountAnnotationsAsync(projectTaskItemIds);
        var consensusCount = await CountConsensusAsync(projectTaskItemIds);
        var averageAgreement = await GetAverageAgreementAsync(projectTaskItemIds);

        return new ProjectOverviewResponse
        {
            ProjectId = project.ProjectId,
            ProjectName = project.Name,
            DatasetItems = datasetItems,
            TasksTotal = taskCounts.Sum(t => t.Count),
            TasksOpened = taskCounts.FirstOrDefault(t => t.Status == LabelingTaskStatus.Opened)?.Count ?? 0,
            TasksClosed = taskCounts.FirstOrDefault(t => t.Status == LabelingTaskStatus.Closed)?.Count ?? 0,
            TasksRemoved = 0,
            AnnotationsTotal = annotationsTotal,
            ConsensusGenerated = consensusCount,
            AgreementAverage = Math.Round(averageAgreement, 4)
        };
    }

    public async Task<DatasetCoverageResponse> GetDatasetCoverageAsync(Guid projectId)
    {
        var datasetItems = await GetProjectTaskItemIdsAsync(projectId);
        var totalDatasetItems = datasetItems.Count();

        var itemsAnnotated = await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId && ti.Annotations.Any())
            .Select(ti => ti.DatasetItemId)
            .Distinct()
            .CountAsync();

        var itemsWithConsensus = await GetConsensusCountAsync(projectId);

        return new DatasetCoverageResponse
        {
            DatasetItems = totalDatasetItems,
            ItemsAnnotated = itemsAnnotated,
            ItemsConsensus = itemsWithConsensus,
            CoveragePercent = totalDatasetItems > 0
                ? Math.Round((double)itemsAnnotated / totalDatasetItems * 100, 2)
                : 0
        };
    }

    public async Task<List<AnnotatorProductivityResponse>> GetAnnotatorProductivityAsync(Guid projectId)
    {
        // TODO: Implement annotator productivity report
        // Should aggregate annotation count and assignment completion per user
        await Task.CompletedTask;
        return new List<AnnotatorProductivityResponse>();
    }

    public async Task<AgreementDistributionResponse> GetAgreementDistributionAsync(Guid projectId)
    {
        var projectTaskItemIds = await GetProjectTaskItemIdsAsync(projectId);
        var scores = await GetAgreementScoresAsync(projectTaskItemIds);

        if (scores.Count == 0)
            return new AgreementDistributionResponse();

        return new AgreementDistributionResponse
        {
            AverageAgreement = Math.Round(scores.Average(), 4),
            HighAgreement = scores.Count(s => s >= 0.8),
            MediumAgreement = scores.Count(s => s >= 0.5 && s < 0.8),
            LowAgreement = scores.Count(s => s < 0.5)
        };
    }

    public async Task<List<ReviewerPerformanceResponse>> GetReviewerPerformanceAsync(Guid projectId)
    {
        var projectTaskItemIds = await GetProjectTaskItemIdsAsync(projectId);

        return await (
            from r in _context.Reviews.AsNoTracking()
            join u in _context.Users on r.ReviewerId equals u.UserId
            where projectTaskItemIds.Contains(r.TaskItemId)
            group r by new { u.UserId, u.DisplayName } into g
            select new ReviewerPerformanceResponse
            {
                ReviewerId = g.Key.UserId,
                DisplayName = g.Key.DisplayName,
                Reviews = g.Count(),
                Approved = g.Count(r => r.Result == ReviewResult.Approved),
                Rejected = g.Count(r => r.Result == ReviewResult.Rejected)
            }
        ).ToListAsync();
    }

    public async Task<List<LabelDistributionResponse>> GetLabelDistributionAsync(Guid projectId)
    {
        var projectTaskItemIds = _context.LabelingTaskItems
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.TaskItemId);

        var payloads = await _context.Annotations
            .AsNoTracking()
            .Where(a => projectTaskItemIds.Contains(a.TaskItemId) && a.Payload != null)
            .Select(a => a.Payload!)
            .ToListAsync();

        var labelCounts = new Dictionary<string, int>();
        foreach (var payload in payloads)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("bboxes", out var bboxes)
                    && bboxes.ValueKind == JsonValueKind.Array)
                {
                    foreach (var bbox in bboxes.EnumerateArray())
                    {
                        if (bbox.TryGetProperty("label", out var labelProp)
                            && labelProp.ValueKind == JsonValueKind.String)
                        {
                            var label = labelProp.GetString()!;
                            labelCounts[label] = labelCounts.GetValueOrDefault(label) + 1;
                        }
                    }
                }
            }
            catch
            {
                // Skip malformed payloads
            }
        }

        return labelCounts
            .Select(kvp => new LabelDistributionResponse { Label = kvp.Key, Count = kvp.Value })
            .OrderByDescending(l => l.Count)
            .ToList();
    }

    // =============== System Statistics ===============

    public async Task<SystemOverviewResponse> GetSystemOverviewAsync()
    {
        return new SystemOverviewResponse
        {
            Users = await _context.Users.CountAsync(),
            Projects = await _context.Projects.CountAsync(),
            Datasets = await _context.Datasets.CountAsync(),
            DatasetItems = await _context.DatasetItems.CountAsync(),
            Annotations = await _context.Annotations.CountAsync(),
            ConsensusGenerated = await _context.Consensuses.CountAsync()
        };
    }

    public async Task<List<ActiveProjectResponse>> GetActiveProjectsAsync()
    {
        var today = DateTime.UtcNow.Date;

        // Join annotations → task items → projects to find activity today
        return await (
            from a in _context.Annotations.AsNoTracking()
            join ti in _context.LabelingTaskItems on a.TaskItemId equals ti.TaskItemId
            join p in _context.Projects on ti.ProjectId equals p.ProjectId
            where a.SubmittedAt >= today
            group a by new { p.ProjectId, p.Name } into g
            select new ActiveProjectResponse
            {
                ProjectId = g.Key.ProjectId,
                Name = g.Key.Name,
                AnnotationsToday = g.Count(),
                ActiveAnnotators = g.Select(a => a.AnnotatorId).Distinct().Count()
            }
        ).ToListAsync();
    }

    public async Task<List<ActivityTimelineResponse>> GetActivityTimelineAsync(int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var data = await _context.Annotations
            .AsNoTracking()
            .Where(a => a.SubmittedAt != null && a.SubmittedAt >= startDate)
            .GroupBy(a => a.SubmittedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return data.Select(d => new ActivityTimelineResponse
        {
            Date = DateOnly.FromDateTime(d.Date),
            Annotations = d.Count
        }).ToList();
    }

    // =============== Query Helpers ===============

    /// <summary>
    /// Get all task item IDs for a project (materialized).
    /// </summary>
    private async Task<IEnumerable<Guid>> GetProjectTaskItemIdsAsync(Guid projectId) =>
        await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.TaskItemId)
            .ToListAsync();

    private async Task<int> CountAnnotationsAsync(IEnumerable<Guid> taskItemIds) =>
        await _context.Annotations
            .AsNoTracking()
            .CountAsync(a => taskItemIds.Contains(a.TaskItemId));

    private async Task<int> CountConsensusAsync(IEnumerable<Guid> taskItemIds) =>
        await _context.Consensuses
            .AsNoTracking()
            .CountAsync(c => taskItemIds.Contains(c.DatasetItemId));

    private async Task<double> GetAverageAgreementAsync(IEnumerable<Guid> taskItemIds)
    {
        var scores = await GetAgreementScoresAsync(taskItemIds);
        return scores.Count > 0 ? scores.Average() : 0;
    }

    private async Task<int> GetConsensusCountAsync(Guid projectId)
    {
        var consensusTaskItemIds = _context.Consensuses.Select(c => c.DatasetItemId);
        return await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId && consensusTaskItemIds.Contains(ti.TaskItemId))
            .Select(ti => ti.DatasetItemId)
            .Distinct()
            .CountAsync();
    }

    private async Task<List<double>> GetAgreementScoresAsync(IEnumerable<Guid> taskItemIds)
    {
        var payloads = await _context.Consensuses
            .AsNoTracking()
            .Where(c => taskItemIds.Contains(c.DatasetItemId))
            .Select(c => c.Payload)
            .ToListAsync();

        return ExtractAgreementScores(payloads);
    }

    /// <summary>
    /// Extracts agreement scores from consensus JSON payloads.
    /// </summary>
    private static List<double> ExtractAgreementScores(List<string> payloads)
    {
        var scores = new List<double>();
        foreach (var payload in payloads)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("agreementScore", out var scoreEl)
                    && scoreEl.TryGetDouble(out var score))
                {
                    scores.Add(score);
                }
            }
            catch
            {
                // Skip malformed payloads
            }
        }
        return scores;
    }
}
