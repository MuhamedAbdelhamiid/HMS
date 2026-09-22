using HMS.Core.Contracts;
using StackExchange.Redis;

namespace HMS.Infrastructure.Repository
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _redisDatabase;
        public CacheRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _redisDatabase = connectionMultiplexer.GetDatabase();
        }
        public async Task<string?> GetCacheAsync(string cacheKey)
        => await _redisDatabase.StringGetAsync(cacheKey);

        public async Task SetCacheAsync(string cacheKey, string cacheValue, TimeSpan ttl)
        => await _redisDatabase.StringSetAsync(cacheKey, cacheValue, ttl);
    }
}
