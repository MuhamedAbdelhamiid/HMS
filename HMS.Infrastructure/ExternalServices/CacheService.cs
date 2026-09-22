using HMS.Core.Contracts;
using HMS.Services.Abstraction;
using System.Text.Json;

namespace HMS.Infrastructure.ExternalServices
{
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;

        public CacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }
        public async Task<string?> GetCacheValueAsync(string key)
        => await _cacheRepository.GetCacheAsync(key);

        public async Task SetCacheValueAsync(string key, object value, TimeSpan ttl)
        {
            var valueAsJson = JsonSerializer.Serialize(value);
            await _cacheRepository.SetCacheAsync(key, valueAsJson, ttl == default ? TimeSpan.FromMinutes(60) : ttl);
        }
    }
}
