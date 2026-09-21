using HMS.Core.Contracts;
using HMS.Core.Entities.SecurityModule;
using HMS.Infrastructure.Data.Context;
using HMS.Infrastructure.Data.DataSeed;
using HMS.Infrastructure.ExternalServices;
using HMS.Infrastructure.ExternalServices.ServiceHelperEntites;
using HMS.Infrastructure.Repository;
using HMS.Services;
using HMS.Services.Abstraction;
using HMS.Services.Profiles.RoomModuleProfiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace HMS.API.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRoomService, RoomService>();
            services.AddTransient<RoomImageValueResolver>();
            services.AddTransient<IAttachmentService, AttachmentService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IFeedbackService, FeedbackService>();

            services.AddHttpClient<IPaymentService, PaymentService>();
            services.AddHttpClient<IAiModerationService, AiModerationService>();

            services.AddAutoMapper(typeof(RoomProfile).Assembly);

            services.AddKeyedScoped<IDataInitializer, IdentityDataInitializer>("Secured");
            services.AddKeyedScoped<IDataInitializer, DataInitializer>("Application");

            services.AddSignalR();

            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityCore<HotelUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration.GetSection("JWTOptions")["Issuer"],
                    ValidAudience = configuration.GetSection("JWTOptions")["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration.GetSection("JWTOptions")["SecretKey"]!)),
                };
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/service"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });



            return services;
        }

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HMS API",
                    Version = "v1",
                    Description = "Hotel Management System API",
                    Contact = new OpenApiContact
                    {
                        Name = "Mohamed Abdelhamid",
                        Email = "muhamedabdelhamiid@gmail.com"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });


                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
                options.DocInclusionPredicate((name, api) => true);
            });

            return services;
        }
    }
}
