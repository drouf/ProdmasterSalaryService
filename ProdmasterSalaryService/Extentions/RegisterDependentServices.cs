using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Classes;
using ProdmasterSalaryService.Services.Hosted;
using ProdmasterSalaryService.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProdmasterSalaryService.Extentions
{
    public static class RegisterDependentServices
    {
        public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddResponseCaching();
            builder.Services.AddControllers(options =>
            {
                options.CacheProfiles.Add("Default30",
                    new CacheProfile()
                    {
                        Duration = 30
                    });
            });

            //Services
            {
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<ICustomService, CustomService>();
                builder.Services.AddScoped<IOperationService, OperationService>();
                builder.Services.AddScoped<IShiftService, ShiftService>();
                builder.Services.AddHostedService<UpdateCustomsHostedService>();
                builder.Services.AddHttpClient<IUpdateCustomsService, UpdateCustomsService>()
                    .SetHandlerLifetime(TimeSpan.FromMinutes(60));
                builder.Services.AddHostedService<UpdateOperationsHostedService>();
                builder.Services.AddHttpClient<IUpdateOperationsService, UpdateOperationsService>()
                    .SetHandlerLifetime(TimeSpan.FromMinutes(1));
                builder.Services.AddHostedService<UpdateShiftsHostedService>();
                builder.Services.AddHttpClient<IUpdateShiftsService, UpdateShiftsService>()
                    .SetHandlerLifetime(TimeSpan.FromMinutes(30));
            }
            //Repository
            {
                builder.Services.AddScoped<UserRepository>();
                builder.Services.AddScoped<CustomRepository>();
                builder.Services.AddScoped<OperationRepository>();
                builder.Services.AddScoped<ShiftRepository>();
            }
            //Database
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                builder.Services.AddDbContext<UserContext>(ConfigureUserContextConnection);
            }

            {
                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options => //CookieAuthenticationOptions
                    {
                        options.LoginPath = new PathString("/account/login");
                    });
            }

            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            void ConfigureUserContextConnection(DbContextOptionsBuilder options)
            {
                options.UseLazyLoadingProxies()
                    .UseNpgsql(builder.Configuration.GetConnectionString("UserContext")).ConfigureWarnings(w => w.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning))
                    .EnableSensitiveDataLogging();
            }

            return builder;
        }
    }
}
