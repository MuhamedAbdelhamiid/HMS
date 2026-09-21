using HMS.Core.Contracts;
using HMS.Core.Entities;
using HMS.Core.Entities.ServiceModule;
using HMS.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HMS.Infrastructure.Data.DataSeed
{
    public class DataInitializer : IDataInitializer
    {
        private readonly AppDbContext _dbContext;

        public DataInitializer(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task InitializeAsync()
        {
            var services = _dbContext.Set<Service>();

            if (!services.Any())
                await SeedDataFromJson<Service, int>("Services.json", services);

            await _dbContext.SaveChangesAsync();
        }
        private async Task SeedDataFromJson<T, TKey>(string fileName, DbSet<T> dbSet)
        where T : BaseEntity<TKey>, new()
        {
            var filePath = @$"..\HMS.Infrastructure\Data\DataSeed\JsonDataSeed\{fileName}";

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Json File Doesn't Exist");

            try
            {
                var fileStream = File.OpenRead(filePath);

                var data = await JsonSerializer.DeserializeAsync<List<T>>(
                    fileStream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (data is not null && data.Count > 0)
                    await dbSet.AddRangeAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding Files Error {ex}");
            }
        }
    }
}
