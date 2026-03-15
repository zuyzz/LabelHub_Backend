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

        // Count dataset items via task items belonging to this project
        var datasetItems = await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.DatasetItemId)
            .Distinct()
            .CountAsync();

        var taskCounts = await _context.LabelingTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        // Get task item IDs for this project to query annotations and consensus
        var projectTaskItemIds = _context.LabelingTaskItems
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.TaskItemId);

        var annotationsTotal = await _context.Annotations
            .AsNoTracking()
            .CountAsync(a => projectTaskItemIds.Contains(a.TaskItemId));

        var consensusCount = await _context.Consensuses
            .AsNoTracking()
            .CountAsync(c => projectTaskItemIds.Contains(c.DatasetItemId));

        // Parse agreement scores from consensus payloads
        var consensusPayloads = await _context.Consensuses
            .AsNoTracking()
            .Where(c => projectTaskItemIds.Contains(c.DatasetItemId))
            .Select(c => c.Payload)
            .ToListAsync();

        var agreementScores = ExtractAgreementScores(consensusPayloads);
        var avgAgreement = agreementScores.Count > 0 ? agreementScores.Average() : 0;

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
            AgreementAverage = Math.Round(avgAgreement, 4)
        };
    }

    public async Task<DatasetCoverageResponse> GetDatasetCoverageAsync(Guid projectId)
    {
        // Total distinct dataset items in this project
        var datasetItems = await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.DatasetItemId)
            .Distinct()
            .CountAsync();

        // Dataset items that have at least one annotation
        var itemsAnnotated = await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId && ti.Annotations.Any())
            .Select(ti => ti.DatasetItemId)
            .Distinct()
            .CountAsync();

        // Dataset items that have a consensus
        var consensusTaskItemIds = _context.Consensuses.Select(c => c.DatasetItemId);
        var itemsConsensus = await _context.LabelingTaskItems
            .AsNoTracking()
            .Where(ti => ti.ProjectId == projectId && consensusTaskItemIds.Contains(ti.TaskItemId))
            .Select(ti => ti.DatasetItemId)
            .Distinct()
            .CountAsync();

        return new DatasetCoverageResponse
        {
            DatasetItems = datasetItems,
            ItemsAnnotated = itemsAnnotated,
            ItemsConsensus = itemsConsensus,
            CoveragePercent = datasetItems > 0
                ? Math.Round((double)itemsAnnotated / datasetItems * 100, 2)
                : 0
        };
    }

    public async Task<List<AnnotatorProductivityResponse>> GetAnnotatorProductivityAsync(Guid projectId)
    {
        // var projectTaskItemIds = _context.LabelingTaskItems
        //     .Where(ti => ti.ProjectId == projectId)
        //     .Select(ti => ti.TaskItemId);

        // var annotatorStats = await (
        //     from a in _context.Annotations.AsNoTracking()
        //     join u in _context.Users on a.AnnotatorId equals u.UserId
        //     where projectTaskItemIds.Contains(a.TaskItemId)
        //     group a by new { u.UserId, u.DisplayName } into g
        //     select new
        //     {
        //         UserId = g.Key.UserId,
        //         DisplayName = g.Key.DisplayName,
        //         Annotations = g.Count()
        //     }
        // ).ToListAsync();

        // // Get task IDs for this project
        // var projectTaskIds = _context.LabelingTasks
        //     .Where(t => t.ProjectId == projectId)
        //     .Select(t => t.TaskId);

        // var assignmentStats = await (
        //     from asgn in _context.Assignments.AsNoTracking()
        //     where projectTaskIds.Contains(asgn.TaskId) && asgn.Status == AssignmentStatus.Completed
        //     group asgn by asgn.AssignedTo into g
        //     select new
        //     {
        //         UserId = g.Key,
        //         CompletedAssignments = g.Count()
        //     }
        // ).ToListAsync();

        // var assignmentLookup = assignmentStats.ToDictionary(a => a.UserId, a => a.CompletedAssignments);

        // return annotatorStats.Select(a => new AnnotatorProductivityResponse
        // {
        //     UserId = a.UserId,
        //     DisplayName = a.DisplayName,
        //     Annotations = a.Annotations,
        //     CompletedAssignments = assignmentLookup.GetValueOrDefault(a.UserId, 0),
        //     AvgTimePerTaskMinutes = null
        // }).ToList();
        return null;
    }

    public async Task<AgreementDistributionResponse> GetAgreementDistributionAsync(Guid projectId)
    {
        var projectTaskItemIds = _context.LabelingTaskItems
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.TaskItemId);

        var consensusPayloads = await _context.Consensuses
            .AsNoTracking()
            .Where(c => projectTaskItemIds.Contains(c.DatasetItemId))
            .Select(c => c.Payload)
            .ToListAsync();

        var scores = ExtractAgreementScores(consensusPayloads);

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
        var projectTaskItemIds = _context.LabelingTaskItems
            .Where(ti => ti.ProjectId == projectId)
            .Select(ti => ti.TaskItemId);

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

    // =============== Helpers ===============

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
