using DataLabelProject.Data.Repositories.Abstractions;
using DataLabelProject.Data.Repositories.Implementations.Categories;
using DataLabelProject.Data.Repositories.Implementations.Guidelines;
using DataLabelProject.Data.Repositories.Implementations.Labels;
using DataLabelProject.Data.Repositories.Implementations.Projects;
using DataLabelProject.Data.Repositories.Implementations.Roles;
using DataLabelProject.Data.Repositories.Implementations.Users;
using DataLabelProject.Data.Repositories.Implementations.AnnotationTasks;
using DataLabelProject.Data.Repositories.Implementations.Annotations;
using DataLabelProject.Business.Services;
using DataLabelProject.Business.Services.Auth;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Business.Services.Roles;
using DataLabelProject.Business.Services.Categories;
using DataLabelProject.Business.Services.Projects;
using DataLabelProject.Business.Services.Labels;
using DataLabelProject.Business.Services.Guidelines;
using DataLabelProject.Business.Services.Datasets;
using DataLabelProject.Business.Services.Storage;
using DataLabelProject.Business.Services.FileUpload;
using DataLabelProject.Business.Services.AnnotationTasks;
using DataLabelProject.Business.Services.Annotations;
using DataLabelProject.Business.Services.Reviews;

namespace DataLabelProject.Infrastructure.Extensions;

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
        services.AddScoped<IAnnotationTaskRepository, AnnotationTaskRepository>();
        services.AddScoped<IAnnotationWorkflowRepository, AnnotationWorkflowRepository>();

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
        services.AddScoped<IAnnotationTaskService, AnnotationTaskService>();
        services.AddScoped<IAnnotationService, AnnotationService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
