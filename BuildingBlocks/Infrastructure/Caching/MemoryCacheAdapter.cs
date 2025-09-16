using Microsoft.Extensions.Caching.Memory;

namespace BuildingBlocks.Infrastructure.Caching;

public sealed class MemoryCacheAdapter : ICache
{
    private readonly IMemoryCache _cache;
    public MemoryCacheAdapter(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) =>
        Task.FromResult(_cache.TryGetValue(key, out var v) ? (T?)v : default);

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        _cache.Set(key, value!, ttl);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
