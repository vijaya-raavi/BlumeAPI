using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Ontec.Core.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache MemoryCache;
        private readonly CacheConfig CacheConfig;
        public CacheService(IMemoryCache memoryCache, IOptions<CacheConfig> cacheConfig)
        {
            MemoryCache = memoryCache;
            if (cacheConfig != null)
            {
                CacheConfig = cacheConfig.Value;
            }
        }

        public void Clear()
        {
            if(MemoryCache is MemoryCache concreteMemoryCache)
            {
                concreteMemoryCache.Clear();
            }
        }

        public T GetOrCreate<T>(string key, Func<T> callback, int? absoluteExpiration)
        {
            return MemoryCache.GetOrCreate(key, cacheEntry =>
            {
                _ = cacheEntry?.SetAbsoluteExpiration(GetAbsoluteExpiration(absoluteExpiration));
                return callback.Invoke();
            });
        }

        public void RemoveByKey(string key)
        {
            MemoryCache.Remove(key);
        }

        public T Set<T>(string key, T value, int? absoluteExpiration)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetAbsoluteExpiration(GetAbsoluteExpiration(absoluteExpiration));
            return MemoryCache.Set(key, value, cacheEntryOptions);
        }

        public T Set<T>(string key, T value, MemoryCacheEntryOptions cacheEntryExtensions)
        {
            return MemoryCache.Set(key, value, cacheEntryExtensions);
        }
        public bool TryGetValue<T>(string key, out T value)
        {
            return MemoryCache.TryGetValue(key, out value);
        }
        private TimeSpan GetAbsoluteExpiration(int? absoluteException)
        {
            return TimeSpan.FromSeconds(absoluteException ?? CacheConfig.AbsoluteExpirations);
        }
    }
}
