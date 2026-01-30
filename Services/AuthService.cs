using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Repositories;
using DataLabel_Project_BE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DataLabel_Project_BE.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public AuthService(IConfiguration configuration, IUserRepository userRepo, IRoleRepository roleRepo)
    {
        _configuration = configuration;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    public static string HashPasswordStatic(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPasswordStatic(password);
        return hashOfInput == passwordHash;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user == null) return null;
        if (!user.IsActive) return null;
        if (!VerifyPassword(request.Password, user.PasswordHash)) return null;

        var role = await _roleRepo.GetByIdAsync(user.RoleId);
        var roleName = role?.RoleName ?? "Unknown";

        var token = GenerateJwtToken(user.UserId, user.Username, roleName);
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        return new LoginResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            RoleName = roleName,
            Token = token,
            ExpiresAt = expiresAt,
            Message = "Login successful"
        };
    }

    private string GenerateJwtToken(Guid userId, string username, string roleName)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
