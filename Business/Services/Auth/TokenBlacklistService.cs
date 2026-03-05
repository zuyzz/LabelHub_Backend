using DataLabelProject.Data;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Business.Services.Auth;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly AppDbContext _dbContext;
    private static bool _tableInitialized;
    private static readonly SemaphoreSlim InitLock = new(1, 1);

    public TokenBlacklistService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RevokeTokenAsync(string jti, DateTime expiresAtUtc)
    {
        await EnsureTableAsync();

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""RevokedToken"" (""jti"", ""expiresAt"", ""revokedAt"")
            VALUES ({jti}, {expiresAtUtc}, {DateTime.UtcNow})
            ON CONFLICT (""jti"")
            DO UPDATE SET ""expiresAt"" = EXCLUDED.""expiresAt"", ""revokedAt"" = EXCLUDED.""revokedAt"";
        ");
    }

    public async Task<bool> IsTokenRevokedAsync(string jti)
    {
        await EnsureTableAsync();

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM ""RevokedToken""
            WHERE ""expiresAt"" <= {DateTime.UtcNow};
        ");

        return await _dbContext.RevokedTokens
            .AsNoTracking()
            .AnyAsync(x => x.Jti == jti && x.ExpiresAt > DateTime.UtcNow);
    }

    private async Task EnsureTableAsync()
    {
        if (_tableInitialized) return;

        await InitLock.WaitAsync();
        try
        {
            if (_tableInitialized) return;

            await _dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""RevokedToken"" (
                    ""jti"" character varying(128) PRIMARY KEY,
                    ""expiresAt"" timestamp with time zone NOT NULL,
                    ""revokedAt"" timestamp with time zone NOT NULL DEFAULT now()
                );
            ");

            _tableInitialized = true;
        }
        finally
        {
            InitLock.Release();
        }
    }
}
