using DataLabel_Project_BE.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Infrastructure
builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Middleware
app.UseSwaggerWithUI();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
