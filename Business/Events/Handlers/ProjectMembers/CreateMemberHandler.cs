using DataLabelProject.Business.Events.Abstraction;
using DataLabelProject.Business.Events.DomainEvents.Project;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Events.Handlers.ProjectConfigs;

public class CreateMemberHandler 
    : IEventHandler<ProjectCreatedEvent>
{
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateMemberHandler(
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService)
    {
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(ProjectCreatedEvent domainEvent)
    {
        var currentUserId = _currentUserService.UserId!.Value;

        var member = new ProjectMember
        {
            ProjectId = domainEvent.ProjectId,
            MemberId = currentUserId,
            JoinedAt = DateTime.UtcNow
        };

        await _memberRepository.CreateAsync(member);
        await _memberRepository.SaveChangesAsync();
    }
}