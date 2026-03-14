using DataLabelProject.Application.DTOs.Statistics;

namespace DataLabelProject.Business.Services.Statistics;

public interface IStatisticsService
{
    // Project-scoped
    Task<ProjectOverviewResponse> GetProjectOverviewAsync(Guid projectId);
    Task<DatasetCoverageResponse> GetDatasetCoverageAsync(Guid projectId);
    Task<List<AnnotatorProductivityResponse>> GetAnnotatorProductivityAsync(Guid projectId);
    Task<AgreementDistributionResponse> GetAgreementDistributionAsync(Guid projectId);
    Task<List<ReviewerPerformanceResponse>> GetReviewerPerformanceAsync(Guid projectId);
    Task<List<LabelDistributionResponse>> GetLabelDistributionAsync(Guid projectId);

    // System-scoped
    Task<SystemOverviewResponse> GetSystemOverviewAsync();
    Task<List<ActiveProjectResponse>> GetActiveProjectsAsync();
    Task<List<ActivityTimelineResponse>> GetActivityTimelineAsync(int days);

    // Authorization helpers
    Task<bool> ProjectExistsAsync(Guid projectId);
    Task<bool> IsProjectMemberAsync(Guid projectId, Guid userId);
}
