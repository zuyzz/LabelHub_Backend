using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DataLabelProject.Data;
using Microsoft.EntityFrameworkCore;

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
                        // Check if user is still active after JWT validation
                        var userId = context.Principal?.FindFirst("sub")?.Value;

                        if (userId == null)
                        {
                            context.Fail("User ID not found in token");
                            return;
                        }

                        var db = context.HttpContext.RequestServices
                            .GetRequiredService<AppDbContext>();

                        var user = await db.Users.FindAsync(Guid.Parse(userId));

                        if (user == null || !user.IsActive)
                        {
                            context.Fail("User account is disabled or not found");
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
