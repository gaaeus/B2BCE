using BuildingBlocks.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class CompanyEndToEndTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CompanyEndToEndTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact(Timeout = 60_000)]
        public async Task RegisterCompany_CreatesOutbox_And_DispatcherProcessesIt()
        {
            // 1) Register company
            var register = new
            {
                legalName = "Integration Acme",
                taxId = "12345678000195",
                email = "test@acme.com"
            };

            var resp = await _client.PostAsJsonAsync("/api/companies", register);
            resp.EnsureSuccessStatusCode();

            // API returns created id in body per controller
            var id = await resp.Content.ReadFromJsonAsync<Guid>();

            id.Should().NotBe(Guid.Empty);

            // 2) Wait for outbox row to be created and processed
            var processed = await WaitForOutboxProcessedAsync(id, timeoutSeconds: 15);

            processed.Should().BeTrue("Outbox dispatcher should process the CompanyRegistered event within the timeout.");
        }

        /// <summary>
        /// Polls the test DB until an OutboxMessage related to the company event is marked processed.
        /// It looks for OutboxMessages whose payload contains the company id (simpler than parsing event types).
        /// </summary>
        private async Task<bool> WaitForOutboxProcessedAsync(Guid companyId, int timeoutSeconds = 10)
        {
            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (DateTime.UtcNow < deadline)
            {
                using var scope = _factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // look for either a processed message or any unprocessed row (so test can fail fast if no row created)
                var processed = db.OutboxMessages
                    .Any(x => x.ProcessedOnUtc != null && x.Payload.Contains(companyId.ToString()));

                if (processed) return true;

                // If not processed but a row exists, wait a bit for dispatcher to run
                var exists = db.OutboxMessages.Any(x => x.Payload.Contains(companyId.ToString()));
                if (!exists)
                {
                    // Outbox row not present yet — wait
                    await Task.Delay(200);
                    continue;
                }

                // row exists but not processed yet
                await Task.Delay(200);
            }

            return false;
        }
    }
}
