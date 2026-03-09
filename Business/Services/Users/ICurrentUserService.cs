namespace DataLabelProject.Business.Services.Users
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
        IEnumerable<string> Roles { get; }
    }
}