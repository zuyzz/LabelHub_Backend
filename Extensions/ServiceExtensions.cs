using DataLabel_Project_BE.Repositories;
using DataLabel_Project_BE.Services;

namespace DataLabel_Project_BE.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectVersionRepository, ProjectVersionRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<ILabelSetRepository, LabelSetRepository>();
        services.AddScoped<IGuidelineRepository, GuidelineRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectVersionService, ProjectVersionService>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<ILabelSetService, LabelSetService>();
        services.AddScoped<IGuidelineService, GuidelineService>();

        return services;
    }
}
