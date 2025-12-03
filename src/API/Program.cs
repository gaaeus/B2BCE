using API.Health;
using API.Middleware;
using API.Security;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Abstractions.Sefaz;
using BuildingBlocks.Domain.Base;
using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Integrations.Sefaz;
using BuildingBlocks.Infrastructure.Messaging;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Persistence.Outbox;
using BuildingBlocks.Infrastructure.Persistence.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;
using System.Text.Json;
using API.Tenancy;
using BuildingBlocks.Application.Abstractions.Tenancy;

// Health checks
// Minimal JSON writer for health responses
static Task WriteHealthJson(HttpContext ctx, HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var payload = new
    {
        status = report.Status.ToString(),
        results = report.Entries.ToDictionary(
            e => e.Key,
            e => new {
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            })
    };
    return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
}


// Build preliminary configuration to feed Serilog
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)       // respects Serilog section in appsettings
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithExceptionDetails()
    .Enrich.FromLogContext()
    .Enrich.With<TenantEnricher>()  // add this
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] ({TenantId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace default logger with Serilog
builder.Host.UseSerilog();
Log.Information("Starting up");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//// EF Core (SQLite dev)
//builder.Services.AddDbContext<AppDbContext>(opt =>
//    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));
// IMPORTANT: register AppDbContext using the overload that accepts IServiceProvider so
// we can resolve the scoped ITenantProvider to get the current tenant id.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var tenantProvider = sp.GetRequiredService<ITenantProvider>();
    var resolver = sp.GetRequiredService<ITenantConnectionResolver>();

    var tenantId = tenantProvider.TenantId; // may be null for global admin
    var connectionString = resolver.GetConnectionString(tenantId);

    // Example with SQLite, but resolver could return Postgres/etc.
    options.UseSqlite(connectionString);

    // If you use interceptors (OutboxInterceptor), add them here
    var interceptor = sp.GetService<BuildingBlocks.Infrastructure.Persistence.Interceptors.OutboxInterceptor>();
    if (interceptor != null) options.AddInterceptors(interceptor);

    // optionally: add sensitive logging in Development only
    var env = sp.GetService<IHostEnvironment>();
    if (env?.IsDevelopment() == true) options.EnableSensitiveDataLogging();
});


builder.Services.Configure<SefazOptions>(builder.Configuration.GetSection("Sefaz"));

var sefazEnabled = builder.Configuration.GetValue<bool>("Sefaz:Enabled");

if (sefazEnabled)
{
    builder.Services.AddHttpClient<ISefazClient, SefazHttpClient>((sp, client) =>
    {
        var opts = sp.GetRequiredService<IOptions<SefazOptions>>().Value;
        client.BaseAddress = new Uri(opts.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}

// Memory cache to demonstrate caching adapter
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICache, MemoryCacheAdapter>();

// FluentValidation v11+
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Application.Companies.RegisterCompanyValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<Application.Companies.AddOrUpdateStateRegistrationValidator>();

// MediatR v12+
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Application.Companies.RegisterCompanyCommand>();
});

// Repositories + UoW
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opts => {
        opts.Authority = builder.Configuration["Auth:Authority"]; // e.g., https://your-idp/
        opts.Audience = builder.Configuration["Auth:Audience"];  // e.g., b2b-api
        opts.RequireHttpsMetadata = true;
    });
builder.Services.AddAuthorization(opts => {
    opts.AddPolicy("SefazQuery", p => p.RequireAuthenticatedUser().RequireClaim("scope", "sefaz.query"));
});

// Outbox (background dispatcher)
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddSingleton<IIntegrationEventPublisher, ConsoleIntegrationEventPublisher>();
builder.Services.AddHostedService<OutboxDispatcher>();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready" })
    .AddCheck<OutboxHealthCheck>("outbox", tags: new[] { "ready" });

// enable accessing HttpContext in handlers
builder.Services.AddHttpContextAccessor();

// register admin key authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, AdminKeyHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.Requirements.Add(new AdminKeyRequirement()));

// tenant provider (scoped so each request has its own tenant)
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddSingleton<TenantRegistry>();

// register resolver
builder.Services.AddSingleton<ITenantConnectionResolver, TenantConnectionResolver>();
builder.Services.AddSingleton<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddScoped<ITenantRegistryService, TenantRegistryService>();

var app = builder.Build();

// Serilog request logging: logs start/stop with timings
app.UseSerilogRequestLogging(opts =>
{
    // Updated code to handle potential null reference for httpCtx.Request.Host.Value
    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
        {
            // enrich with useful items per-request
            var requestHost = httpCtx.Request.Host.Value ?? "unknown";
            diagCtx.Set("RequestHost", requestHost);
            diagCtx.Set("RequestScheme", httpCtx.Request.Scheme);
            diagCtx.Set("User", httpCtx.User?.Identity?.Name ?? "anonymous");
            // CorrelationId is set by middleware below and picked from LogContext
        };
    });
});

// Correlation Id middleware (pushes CorrelationId into LogContext)
app.UseMiddleware<CorrelationIdMiddleware>();

// Optional: custom exception mapping middleware you've already added
app.UseMiddleware<ExceptionMappingMiddleware>();

// insert tenant resolution early in pipeline
app.UseMiddleware<TenantResolutionMiddleware>();


// Liveness: app is up
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false, // no checks; just 200 if process is running
    ResponseWriter = WriteHealthJson
});

// Readiness: dependencies (DB, etc.)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = WriteHealthJson
});
// End health checks

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMappingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHttpsRedirection();

await app.RunAsync();
