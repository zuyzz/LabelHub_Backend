using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DataLabel_Project_BE.Extensions;

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
                    }
                };
            });

        services.AddAuthorization();
        
        return services;
    }
}
