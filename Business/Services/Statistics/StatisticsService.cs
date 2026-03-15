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
        // var project = await _context.Projects
        //     .AsNoTracking()
        //     .Where(p => p.ProjectId == projectId)
        //     .Select(p => new { p.ProjectId, p.Name })
        //     .FirstAsync();

        // var datasetItems = await _context.DatasetItems
        //     .AsNoTracking()
        //     .CountAsync(di => _context.ProjectDatasets
        //         .Any(pd => pd.ProjectId == projectId && pd.DatasetId == di.DatasetId));

        // var taskCounts = await _context.LabelingTasks
        //     .AsNoTracking()
        //     .Where(t => t.ProjectId == projectId)
        //     .GroupBy(t => t.Status)
        //     .Select(g => new { Status = g.Key, Count = g.Count() })
        //     .ToListAsync();

        // var projectTaskIds = _context.LabelingTasks
        //     .Where(t => t.ProjectId == projectId)
        //     .Select(t => t.TaskId);

        // var annotationsTotal = await _context.Annotations
        //     .AsNoTracking()
        //     .CountAsync(a => projectTaskIds.Contains(a.TaskId));

        // var consensusStats = await _context.Consensuses
        //     .AsNoTracking()
        //     .Where(c => projectTaskIds.Contains(c.TaskId))
        //     .GroupBy(_ => 1)
        //     .Select(g => new { Count = g.Count(), AvgScore = g.Average(c => c.AgreementScore) })
        //     .FirstOrDefaultAsync();

        // return new ProjectOverviewResponse
        // {
        //     ProjectId = project.ProjectId,
        //     ProjectName = project.Name,
        //     DatasetItems = datasetItems,
        //     TasksTotal = taskCounts.Sum(t => t.Count),
        //     TasksOpened = taskCounts.FirstOrDefault(t => t.Status == LabelingTaskStatus.Opened)?.Count ?? 0,
        //     TasksClosed = taskCounts.FirstOrDefault(t => t.Status == LabelingTaskStatus.Closed)?.Count ?? 0,
        //     TasksRemoved = taskCounts.FirstOrDefault(t => t.Status == LabelingTaskStatus.Removed)?.Count ?? 0,
        //     AnnotationsTotal = annotationsTotal,
        //     ConsensusGenerated = consensusStats?.Count ?? 0,
        //     AgreementAverage = Math.Round(consensusStats?.AvgScore ?? 0, 4)
        // };
        return null;
    }

    public async Task<DatasetCoverageResponse> GetDatasetCoverageAsync(Guid projectId)
    {
        // var datasetItems = await _context.DatasetItems
        //     .AsNoTracking()
        //     .CountAsync(di => _context.ProjectDatasets
        //         .Any(pd => pd.ProjectId == projectId && pd.DatasetId == di.DatasetId));

        // var itemsAnnotated = await _context.LabelingTasks
        //     .AsNoTracking()
        //     .Where(t => t.ProjectId == projectId && t.Annotations.Any())
        //     .Select(t => t.DatasetItemId)
        //     .Distinct()
        //     .CountAsync();

        // var consensusTaskIds = _context.Consensuses.Select(c => c.TaskId);
        // var itemsConsensus = await _context.LabelingTasks
        //     .AsNoTracking()
        //     .Where(t => t.ProjectId == projectId && consensusTaskIds.Contains(t.TaskId))
        //     .Select(t => t.DatasetItemId)
        //     .Distinct()
        //     .CountAsync();

        // return new DatasetCoverageResponse
        // {
        //     DatasetItems = datasetItems,
        //     ItemsAnnotated = itemsAnnotated,
        //     ItemsConsensus = itemsConsensus,
        //     CoveragePercent = datasetItems > 0
        //         ? Math.Round((double)itemsAnnotated / datasetItems * 100, 2)
        //         : 0
        // };
        return null;
    }

    public async Task<List<AnnotatorProductivityResponse>> GetAnnotatorProductivityAsync(Guid projectId)
    {
        // var annotatorStats = await (
        //     from a in _context.Annotations.AsNoTracking()
        //     join t in _context.LabelingTasks on a.TaskId equals t.TaskId
        //     join u in _context.Users on a.AnnotatorId equals u.UserId
        //     where t.ProjectId == projectId
        //     group a by new { u.UserId, u.DisplayName } into g
        //     select new
        //     {
        //         UserId = g.Key.UserId,
        //         DisplayName = g.Key.DisplayName,
        //         Annotations = g.Count()
        //     }
        // ).ToListAsync();

        // var assignmentStats = await (
        //     from asgn in _context.Assignments.AsNoTracking()
        //     join t in _context.LabelingTasks on asgn.TaskId equals t.TaskId
        //     where t.ProjectId == projectId && asgn.Status == AssignmentStatus.Completed
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
        // var scores = await (
        //     from c in _context.Consensuses.AsNoTracking()
        //     join t in _context.LabelingTasks on c.TaskId equals t.TaskId
        //     where t.ProjectId == projectId
        //     select c.AgreementScore
        // ).ToListAsync();

        // if (scores.Count == 0)
        //     return new AgreementDistributionResponse();

        // return new AgreementDistributionResponse
        // {
        //     AverageAgreement = Math.Round(scores.Average(), 4),
        //     HighAgreement = scores.Count(s => s >= 0.8),
        //     MediumAgreement = scores.Count(s => s >= 0.5 && s < 0.8),
        //     LowAgreement = scores.Count(s => s < 0.5)
        // };
        return null;
    }

    public async Task<List<ReviewerPerformanceResponse>> GetReviewerPerformanceAsync(Guid projectId)
    {
        // return await (
        //     from r in _context.Reviews.AsNoTracking()
        //     join t in _context.LabelingTasks on r.TaskId equals t.TaskId
        //     join u in _context.Users on r.ReviewerId equals u.UserId
        //     where t.ProjectId == projectId
        //     group r by new { u.UserId, u.DisplayName } into g
        //     select new ReviewerPerformanceResponse
        //     {
        //         ReviewerId = g.Key.UserId,
        //         DisplayName = g.Key.DisplayName,
        //         Reviews = g.Count(),
        //         Approved = g.Count(r => r.Result == ReviewResult.Approved),
        //         Rejected = g.Count(r => r.Result == ReviewResult.Rejected)
        //     }
        // ).ToListAsync();
        return null;
    }

    public async Task<List<LabelDistributionResponse>> GetLabelDistributionAsync(Guid projectId)
    {
        // var payloads = await (
        //     from a in _context.Annotations.AsNoTracking()
        //     join t in _context.LabelingTasks on a.TaskId equals t.TaskId
        //     where t.ProjectId == projectId
        //     select a.Payload
        // ).ToListAsync();

        // var labelCounts = new Dictionary<string, int>();
        // foreach (var payload in payloads)
        // {
        //     try
        //     {
        //         using var doc = JsonDocument.Parse(payload);
        //         if (doc.RootElement.TryGetProperty("bboxes", out var bboxes)
        //             && bboxes.ValueKind == JsonValueKind.Array)
        //         {
        //             foreach (var bbox in bboxes.EnumerateArray())
        //             {
        //                 if (bbox.TryGetProperty("label", out var labelProp)
        //                     && labelProp.ValueKind == JsonValueKind.String)
        //                 {
        //                     var label = labelProp.GetString()!;
        //                     labelCounts[label] = labelCounts.GetValueOrDefault(label) + 1;
        //                 }
        //             }
        //         }
        //     }
        //     catch
        //     {
        //         // Skip malformed payloads
        //     }
        // }

        // return labelCounts
        //     .Select(kvp => new LabelDistributionResponse { Label = kvp.Key, Count = kvp.Value })
        //     .OrderByDescending(l => l.Count)
        //     .ToList();
        return null;
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
        // var today = DateTime.UtcNow.Date;

        // return await (
        //     from a in _context.Annotations.AsNoTracking()
        //     join t in _context.LabelingTasks on a.TaskId equals t.TaskId
        //     join p in _context.Projects on t.ProjectId equals p.ProjectId
        //     where a.SubmittedAt >= today
        //     group a by new { p.ProjectId, p.Name } into g
        //     select new ActiveProjectResponse
        //     {
        //         ProjectId = g.Key.ProjectId,
        //         Name = g.Key.Name,
        //         AnnotationsToday = g.Count(),
        //         ActiveAnnotators = g.Select(a => a.AnnotatorId).Distinct().Count()
        //     }
        // ).ToListAsync();
        return null;
    }

    public async Task<List<ActivityTimelineResponse>> GetActivityTimelineAsync(int days)
    {
        // var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // var data = await _context.Annotations
        //     .AsNoTracking()
        //     .Where(a => a.SubmittedAt != null && a.SubmittedAt >= startDate)
        //     .GroupBy(a => a.SubmittedAt!.Value.Date)
        //     .Select(g => new { Date = g.Key, Count = g.Count() })
        //     .OrderBy(x => x.Date)
        //     .ToListAsync();

        // return data.Select(d => new ActivityTimelineResponse
        // {
        //     Date = DateOnly.FromDateTime(d.Date),
        //     Annotations = d.Count
        // }).ToList();
        return null;
    }
}
