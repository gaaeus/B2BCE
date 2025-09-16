var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Memory cache to demonstrate caching adapter
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<BuildingBlocks.Infrastructure.Caching.ICache,
                              BuildingBlocks.Infrastructure.Caching.MemoryCacheAdapter>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync();
