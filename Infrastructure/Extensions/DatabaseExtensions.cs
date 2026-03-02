using DataLabelProject.Business.Services.Storage;
using DataLabelProject.Data;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured");

        services.AddDbContext<AppDbContext>(options =>
        {
            // Enable retry on failure for transient errors
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsqlOptions.CommandTimeout(30);
            });

            // Enable sensitive data logging in Development only
            if (env.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SupabaseStorageOptions>(
            configuration.GetSection("SupabaseStorage"));

        services.AddHttpClient<IFileStorage, SupabaseFileStorage>();

        return services;
    }
}
