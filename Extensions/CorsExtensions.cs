using Microsoft.Extensions.DependencyInjection;

namespace DataLabel_Project_BE.Extensions;

public static class CorsExtensions
{
    private const string DefaultCorsPolicy = "DefaultCorsPolicy";

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, policy =>
            {
                if (allowedOrigins == null || allowedOrigins.Length == 0)
                {
                    // Development / fallback: allow any origin
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }

    public static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseCors(DefaultCorsPolicy);
        return app;
    }
}
