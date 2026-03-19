using DataLabelProject.Business.Events.Abstraction;
using DataLabelProject.Business.Events.DomainEvents.Project;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Events.Handlers.ProjectConfigs;

public class CreateMemberHandler 
    : IEventHandler<ProjectCreatedEvent>
{
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public CreateMemberHandler(
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task HandleAsync(ProjectCreatedEvent domainEvent)
    {
        var currentUserId = _currentUserService.UserId!.Value;

        var existingMember = await _memberRepository.GetByIdAsync(domainEvent.ProjectId, currentUserId);
        if (existingMember == null)
        {
            var member = new ProjectMember
            {
                ProjectId = domainEvent.ProjectId,
                MemberId = currentUserId,
                JoinedAt = DateTime.UtcNow
            };

            await _memberRepository.CreateAsync(member);
            await _memberRepository.SaveChangesAsync();
        }

        var activityLog = new ActivityLog
        {
            ProjectId = domainEvent.ProjectId,
            UserId = currentUserId,
            EventType = "PROJECT_MEMBER_AUTO_ADDED",
            TargetEntity = "ProjectMember",
            TargetId = currentUserId,
            Details = $"{{\"message\":\"Manager was automatically added to project annotation team on project creation\",\"projectId\":\"{domainEvent.ProjectId}\",\"memberId\":\"{currentUserId}\"}}",
            CreatedAt = DateTime.UtcNow
        };

        await _context.ActivityLogs.AddAsync(activityLog);
        await _context.SaveChangesAsync();
    }
}