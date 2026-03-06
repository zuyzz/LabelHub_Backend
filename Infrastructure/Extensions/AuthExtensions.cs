using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DataLabelProject.Business.Services.Auth;
using DataLabelProject.Data.Repositories.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DataLabelProject.Infrastructure.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key not configured");

        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(token) &&
                            !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        // Check 1: token revocation blacklist (BR-logout)
                        var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                        var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        if (!string.IsNullOrWhiteSpace(jti) && await blacklist.IsTokenRevokedAsync(jti))
                        {
                            context.Fail("Token has been revoked");
                            return;
                        }

                        // Check 2: user still active (BR-64)
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (Guid.TryParse(userIdClaim, out var userId))
                        {
                            var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                            var user = await userRepo.GetByIdAsync(userId);
                            if (user == null || !user.IsActive)
                            {
                                context.Fail("Account has been deactivated");
                            }
                        }
                    }
                };
            });

        services.AddAuthorization();
        
        return services;
    }
}
