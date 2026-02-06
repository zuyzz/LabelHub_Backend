using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Services;
using DataLabel_Project_BE.Services.Storage;
using DataLabel_Project_BE.Services.Uploads;
using DataLabel_Project_BE.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database Configuration (Supabase Session Pooler)
// =======================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' is not configured");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Enable retry on failure for transient errors
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        
        // Set command timeout
        npgsqlOptions.CommandTimeout(30);
    });
    
    // Enable sensitive data logging in Development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// =======================
// Register services
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<ILabelSetRepository, LabelSetRepository>();
builder.Services.AddScoped<IGuidelineRepository, GuidelineRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<ILabelSetService, LabelSetService>();
builder.Services.AddScoped<IGuidelineService, GuidelineService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =======================
// Configure JWT Authentication
// =======================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");

// Warn if using placeholder key (safe for GitHub but needs real key for production)
if (jwtKey == "CHANGE_ME_FOR_LOCAL_DEVELOPMENT")
{
    Console.WriteLine("⚠️  WARNING: Using placeholder JWT key. Copy appsettings.json to appsettings.Development.json and set a real secret key for local development.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
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

    // Tự động xử lý token không cần "Bearer " prefix
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                // Tự động thêm "Bearer " nếu chưa có
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = token;
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// =======================
// Configure Swagger
// =======================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DataLabel Project API",
        Version = "v1",
        Description = "Backend API cho hệ thống Data Labeling – Quản lý người dùng & phân quyền"
    });

    // XML comments (English descriptions, no emojis)
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // JWT Security Scheme
    options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",              // QUAN TRỌNG: phải là "bearer"
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "🔑 Chỉ cần dán TOKEN vào đây (không cần gõ 'Bearer')"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "JWT"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =======================
// Middleware pipeline
// =======================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DataLabel API v1");
    options.DocumentTitle = "DataLabel API Documentation";
});

app.UseHttpsRedirection();

app.UseAuthentication(); // ⚠️ PHẢI trước Authorization
app.UseAuthorization();

app.MapControllers();
app.Run();
