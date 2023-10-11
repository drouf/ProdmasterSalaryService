using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Repositories;
using ProdmasterSalaryService.Services.Classes;
using ProdmasterSalaryService.Services.Hosted;
using ProdmasterSalaryService.Services.Interfaces;
using ProdmasterSalaryService.ViewModels.Account;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRouter(r =>
{
    r.MapGet(".well-known/acme-challenge/{id}", async (request, response, routeData) =>
    {
        var id = routeData.Values["id"] as string;
        var file = Path.Combine(app.Environment.WebRootPath, ".well-known", "acme-challenge", id);
        await response.SendFileAsync(file);
    });
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
