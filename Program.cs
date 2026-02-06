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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

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

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<ILabelService, LabelService>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();

// Configure multipart form limits to allow larger uploads (512 MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 536870912; // 512 MB
    options.MultipartHeadersLengthLimit = 16384;  // 16 KB headers
});

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


// Repositories & services
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Dataset import related registrations
builder.Services.AddScoped<IDatasetRepository, DatasetRepository>();
builder.Services.AddScoped<IDatasetService, DatasetService>();

// File storage: prefer Supabase when configured, otherwise fall back to local storage.
// Bind Supabase options from configuration.
builder.Services.Configure<SupabaseStorageOptions>(builder.Configuration.GetSection("SupabaseStorage"));

// Register SupabaseFileStorage as a typed HttpClient-backed service. The typed client is created with the configured base URL and API key.
builder.Services.AddHttpClient<IFileStorage, SupabaseFileStorage>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<SupabaseStorageOptions>>().Value;

    if (string.IsNullOrWhiteSpace(opts.Url))
        throw new InvalidOperationException("SupabaseStorage:Url is missing");

    if (!Uri.TryCreate(opts.Url, UriKind.Absolute, out var uri))
        throw new InvalidOperationException("SupabaseStorage:Url is invalid");

    // Optional (recommended if using relative URLs)
    // client.BaseAddress = uri;

    client.Timeout = TimeSpan.FromMinutes(5);

    // 🔑 REQUIRED by Supabase Storage
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", opts.ApiKey);

    client.DefaultRequestHeaders.TryAddWithoutValidation("apikey", opts.ApiKey);
});


// Use Supabase-backed storage as the IFileStorage implementation.
builder.Services.AddScoped<IFileStorage, SupabaseFileStorage>();

// Register upload strategies: archive first (so it can detect zip/rar), then text/image
builder.Services.AddScoped<ArchiveUploadStrategy>();
builder.Services.AddScoped<TextUploadStrategy>();
builder.Services.AddScoped<ImageUploadStrategy>();
// Expose strategies as IFileUploadStrategy via DI (order doesn't matter, Archive will handle zip/rar)
builder.Services.AddScoped<IFileUploadStrategy>(sp => sp.GetRequiredService<ArchiveUploadStrategy>());
builder.Services.AddScoped<IFileUploadStrategy>(sp => sp.GetRequiredService<TextUploadStrategy>());
builder.Services.AddScoped<IFileUploadStrategy>(sp => sp.GetRequiredService<ImageUploadStrategy>());

// Accessor used to read current user info in services
builder.Services.AddHttpContextAccessor();

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

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
