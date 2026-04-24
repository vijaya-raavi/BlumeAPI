using Microsoft.Extensions.Caching.Memory;
namespace Ontec.Core.Cache
{
    public interface ICacheService
    {
        bool TryGetValue<T>(string key, out T value);
        T Set<T>(string key, T value, int? absoluteExpiration = null);
        T Set<T>(string key, T value, MemoryCacheEntryOptions cacheEntryExtensions);
        T GetOrCreate<T>(string key, Func<T> callback, int? absoluteExpiration = null);
        void RemoveByKey(string key);
        void Clear();
    }
}
