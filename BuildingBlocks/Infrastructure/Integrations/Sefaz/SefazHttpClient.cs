using System.Net.Http.Json;
using BuildingBlocks.Application.Abstractions.Sefaz;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Integrations.Sefaz;

public sealed class SefazOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public bool UseClientCertificate { get; set; } = false;
}

/// <summary>
/// HTTP client for querying SEFAZ state web services about company registrations.
/// </summary>
public sealed class SefazHttpClient : ISefazClient
{
    private readonly HttpClient _http;
    private readonly ILogger<SefazHttpClient> _logger;
    private readonly SefazOptions _opts;

    public SefazHttpClient(HttpClient http, IOptions<SefazOptions> opts, ILogger<SefazHttpClient> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task<SefazResult> QueryAsync(string cnpj, string uf, string ie, CancellationToken ct = default)
    {
        var url = $"{_opts.BaseUrl.TrimEnd('/')}/consulta-inscricao?cnpj={cnpj}&uf={uf}&ie={ie}";

        _logger.LogInformation("Querying SEFAZ {Uf} for CNPJ={Cnpj}, IE={IE}", uf, cnpj, ie);
        using var resp = await _http.GetAsync(url, ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("SEFAZ {Uf} returned {StatusCode}", uf, resp.StatusCode);
            return new SefazResult("UNKNOWN", "UNKNOWN", DateTimeOffset.UtcNow);
        }

        var dto = await resp.Content.ReadFromJsonAsync<SefazResponseDto>(cancellationToken: ct)
                  ?? new SefazResponseDto { Status = "UNKNOWN", RegimeTributario = "UNKNOWN" };

        return new SefazResult(dto.Status, dto.RegimeTributario, dto.CheckedAt);
    }

    private sealed class SefazResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public string RegimeTributario { get; set; } = string.Empty;
        public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
