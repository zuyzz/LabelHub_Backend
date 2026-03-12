using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task CreateAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task SaveChangesAsync();
}
