namespace HMS.Services.Abstraction
{
    public interface ICacheService
    {
        Task<string?> GetCacheValueAsync(string key);
        Task SetCacheValueAsync(string key, object value, TimeSpan ttl);
    }
}
