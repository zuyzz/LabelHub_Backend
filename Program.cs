using DataLabel_Project_BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Register services
// =======================
builder.Services.AddSingleton<AuthService>(); // Singleton for in-memory mock data
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
        )
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

    // XML comments (Swagger tiếng Việt)
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // 🔐 JWT Security Scheme (CHỈ DÁN TOKEN – KHÔNG CẦN 'Bearer')
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
