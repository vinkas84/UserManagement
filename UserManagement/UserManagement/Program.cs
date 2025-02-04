using Infrastructure;
using Infrastructure.Interfaces;
using Business.Interfaces;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Logs to console
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.WithProperty("ApiCall"))
        .WriteTo.File("logs/generalLog-.txt", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty("ApiCall"))
        .WriteTo.File("logs/apiLog-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

// Add Serilog as the logging provider
builder.Host.UseSerilog();

// Add services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Define the security scheme for the API Key
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header, // The API key will be passed in the header
        Name = "X-API-Key", // The header key name
        Type = SecuritySchemeType.ApiKey, // Define that it's an API key
        Description = "API Key Authentication"
    });

    // Add security requirement for endpoints that need API key
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                Array.Empty<string>()
            }
        });
}); ;

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware > (); // Register Exception Handler Middleware
app.UseMiddleware<LoggingMiddleware>(); // Register the logging middleware
app.UseMiddleware<ApiKeyMiddleware>(); // Register API Key Middleware

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Ensure logs are flushed when the app shuts down
Log.CloseAndFlush();
