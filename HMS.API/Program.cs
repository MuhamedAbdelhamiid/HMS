using HMS.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

using HMS.API.Extensions;
using HMS.Core.Contracts;
using HMS.Infrastructure.Repository;

namespace HMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region DI Registeration

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            var app = builder.Build();

            #region Database Migration
            await app.MigrateDatabaseAsync();
            #endregion

            #region Middleware Configurations
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers(); 
            #endregion

            app.Run();
        }
    }
}
