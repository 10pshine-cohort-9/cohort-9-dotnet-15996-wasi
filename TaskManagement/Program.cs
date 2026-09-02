using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Middlewares;
using TaskAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---> SERILOG SETUP <---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/TaskAPI-Log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---> JWT AUTHENTICATION SETUP <---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key appsettings.json mein missing hai!")))
        };
    });

builder.Services.AddControllers();
builder.Services.AddScoped<ITaskService, TaskService>();

// ---> SWAGGER KI SETTINGS <---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---> BROWSER WALA UI <---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Task Management API start ho rahi hai...");

    // SonarQube Fix: Await RunAsync instead
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API start hote waqt crash ho gayi!");
}
finally
{
    // SonarQube Fix: Await CloseAndFlushAsync instead
    await Log.CloseAndFlushAsync();
}