using Microsoft.Extensions.Caching.Memory;

namespace Quingo.Infrastructure;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T data);
    void Remove(string key);
}

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private const int CacheExpirationHours = 3;
    
    public T? Get<T>(string key)
    {
        return cache.Get<T>(key);
    }
    
    public void Set<T>(string key, T data)
    {
        cache.Set(key, data, TimeSpan.FromHours(CacheExpirationHours));
    }
    
    public void Remove(string key)
    {
        cache.Remove(key);
    }
}