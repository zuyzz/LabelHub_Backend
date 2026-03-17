using DataLabelProject.Infrastructure.Extensions;
using DataLabelProject.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Infrastructure
builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddObjectStorage(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

// Middleware
app.UseSwaggerWithUI();
app.UseHttpsRedirection();
app.UseRouting();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
