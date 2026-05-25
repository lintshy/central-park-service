using System.Text;
using Asp.Versioning;
using CentralPark.Api.Extensions;
using CentralPark.Api.Middleware;
using CentralPark.Application.Extensions;
using CentralPark.Infrastructure.Extensions;
using CentralPark.Infrastructure.Persistence;
using CentralPark.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using CentralPark.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services
    .AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


builder.Services.AddApplicationAuthorization();

// API Versioning
builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new ApiVersion(1);
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.ReportApiVersions = true;
}).AddApiExplorer(opts =>
{
    opts.GroupNameFormat = "'v'V";
    opts.SubstituteApiVersionInUrl = true;
});

// Controllers
builder.Services.AddControllers();

// OpenAPI (Scalar)
builder.Services.AddOpenApi();

// Rate limiting — Auth endpoints
builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter(RateLimitPolicies.Auth, limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
    });
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// CORS — explicit origins only; AllowAnyOrigin forbidden in non-dev
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply pending migrations and seed dev data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
        await seeder.SeedAsync();
    }
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
app.Lifetime.ApplicationStarted.Register(() =>
{
    StartupLog.ServiceStarted(startupLogger, app.Environment.EnvironmentName);
    foreach (var url in app.Urls)
    {
        StartupLog.ListeningOn(startupLogger, url);
        if (app.Environment.IsDevelopment())
        {
            StartupLog.ScalarUi(startupLogger, url);
            StartupLog.OpenApiSpec(startupLogger, url);
        }
    }
});

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program;

internal static partial class StartupLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Central Park Service started | Environment: {Environment}")]
    internal static partial void ServiceStarted(Microsoft.Extensions.Logging.ILogger logger, string environment);

    [LoggerMessage(Level = LogLevel.Information, Message = "Listening on {Url}")]
    internal static partial void ListeningOn(Microsoft.Extensions.Logging.ILogger logger, string url);

    [LoggerMessage(Level = LogLevel.Information, Message = "Scalar UI    -> {BaseUrl}/scalar/v1")]
    internal static partial void ScalarUi(Microsoft.Extensions.Logging.ILogger logger, string baseUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "OpenAPI spec -> {BaseUrl}/openapi/v1.json")]
    internal static partial void OpenApiSpec(Microsoft.Extensions.Logging.ILogger logger, string baseUrl);
}
