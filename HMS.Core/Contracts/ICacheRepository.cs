namespace HMS.Core.Contracts
{
    public interface ICacheRepository
    {
        Task<string?> GetCacheAsync(string cacheKey);
        Task SetCacheAsync(string cacheKey, string cacheValue, TimeSpan ttl);
    }
}
