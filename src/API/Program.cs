
using API.Middleware;
using BuildingBlocks.Domain.Base;
using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Persistence.Repositories;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using BuildingBlocks.Infrastructure.Persistence.Outbox;

using BuildingBlocks.Application.Abstractions.Sefaz;
using BuildingBlocks.Infrastructure.Integrations.Sefaz;

using Microsoft.EntityFrameworkCore;

using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// EF Core (SQLite dev)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));

// Sefaz client stub (replace with real implementation)
builder.Services.AddScoped<
    BuildingBlocks.Application.Abstractions.Sefaz.ISefazClient,
    BuildingBlocks.Infrastructure.Integrations.Sefaz.SefazClientStub>();


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
builder.Services.AddScoped<ISefazClient, SefazClientStub>(); // When ready to go real, swap SefazClientStub with an HTTPS client (certificate auth) that calls SEFAZ endpoints, but the Application code stays identical.

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


var app = builder.Build();

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
