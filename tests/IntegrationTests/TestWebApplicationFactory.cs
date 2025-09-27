using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace IntegrationTests
{
    /// <summary>
    /// Creates an in-memory SQLite-backed host for integration tests.
    /// It replaces the AppDbContext registration so all tests use the same in-memory DB.
    /// </summary>
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Remove the existing AppDbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add an AppDbContext using the shared in-memory connection
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Build the service provider and initialize the DB
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory>>();
                    try
                    {
                        var db = scopedServices.GetRequiredService<AppDbContext>();
                        // apply migrations
                        db.Database.EnsureDeleted();
                        db.Database.EnsureCreated();
                        // If you prefer migrations:
                        // db.Database.Migrate();
                        logger.LogInformation("Test DB created in-memory.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred creating the test DB.");
                        throw;
                    }
                }
            });
        }

        public new void Dispose()
        {
            base.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
