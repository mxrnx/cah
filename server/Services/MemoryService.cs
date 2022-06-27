using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

public class MemoryService
{
    private readonly MemoryCache _cache;
    
    public MemoryService()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void SetCzar(Guid id) =>
        _cache.Set("czar", id);

    public Guid GetCzar() =>
        _cache.TryGetValue<Guid>("czar", out var czar)
            ? czar
            : Guid.Empty;
}
