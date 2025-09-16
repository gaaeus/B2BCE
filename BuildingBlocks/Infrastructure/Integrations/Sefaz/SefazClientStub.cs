
using BuildingBlocks.Application.Abstractions.Sefaz;

namespace BuildingBlocks.Infrastructure.Integrations.Sefaz
{
    public sealed class SefazClientStub : ISefazClient
    {
        public Task<SefazResult> QueryAsync(string cnpj, string uf, string ie, CancellationToken ct = default) =>
          Task.FromResult(new SefazResult("HABILITADO", "NORMAL", DateTimeOffset.UtcNow));
    }
}
