using HMS.API.Extensions;
using HMS.Core.Contracts;
using HMS.Infrastructure.Data.Context;
using HMS.Infrastructure.Repository;
using HMS.Services;
using HMS.Services.Abstraction;
using HMS.Services.Profiles.RoomModuleProfiles;
using Microsoft.EntityFrameworkCore;

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
                opt.UseSqlServer(
                    builder
                    .Configuration
                    .GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddTransient<RoomImageValueResolver>();
            builder.Services.AddTransient<IAttachmentService, AttachmentService>();

            builder.Services.AddAutoMapper(typeof(RoomProfile).Assembly);

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

            app.UseStaticFiles();
            app.UseAuthorization();


            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
