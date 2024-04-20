using NLog;
using NLog.Web;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignalR.Redis.Data;
using SignalR.Redis.Models;
using SignalR.Redis.Hubs;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

    // Early init of NLog to allow startup and exception logging, before host is built
    //logger.Debug("init main");

    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

    builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var redisConnection = builder.Configuration
        .GetConnectionString("Redis");


        options.Configuration = redisConnection;
        options.InstanceName = "Redis_SignalR_";
    });


    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddSignalR();
    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }
    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.MapHub<ChatHub>("/chatHub");
    app.MapRazorPages();
    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}