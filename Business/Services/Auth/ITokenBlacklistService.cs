namespace DataLabelProject.Business.Services.Auth;

public interface ITokenBlacklistService
{
    Task RevokeTokenAsync(string jti, DateTime expiresAtUtc);
    Task<bool> IsTokenRevokedAsync(string jti);
}
