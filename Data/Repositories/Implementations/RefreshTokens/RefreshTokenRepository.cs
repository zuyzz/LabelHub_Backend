using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.RefreshTokens;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task CreateAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow));
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
