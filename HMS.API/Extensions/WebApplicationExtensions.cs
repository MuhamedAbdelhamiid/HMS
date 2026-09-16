using HMS.Core.Contracts;
using HMS.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HMS.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication application)
        {
            await using var scope = application.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();


            if (pendingMigrations.Any())
                await dbContext.Database.MigrateAsync();

            return application;
        }

        public static async Task<WebApplication> IdentitySeedAsync(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();

            var IdentityDataInitializer = scope.ServiceProvider.GetRequiredKeyedService<IDataInitializer>("Secured");

            await IdentityDataInitializer.InitializeAsync();

            return app;
        }
    }
}
