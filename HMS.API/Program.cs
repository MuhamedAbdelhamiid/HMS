using HMS.API.Extensions;
using HMS.Core.Contracts;
using HMS.Infrastructure.Data.DataSeed;
using HMS.Infrastructure.ExternalServices.Hubs;

namespace HMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region DI Registeration

            builder.Services.AddControllers();

            builder.Services.AddSwaggerServices();
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddKeyedScoped<IDataInitializer, IdentityDataInitializer>("Secured");



            #endregion

            var app = builder.Build();

            #region Database Migration & Seeding

            await app.MigrateDatabaseAsync();
            await app.IdentitySeedAsync();
            #endregion

            #region Middleware Configurations
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<ServiceHub>("/service");
            #endregion

            app.Run();
        }
    }
}
