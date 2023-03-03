using Contracts;
using Contracts.DbContext;
using Contracts.Repositories;
using Contracts.Service;
using DataAccess;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PluginLibrary.PluginRepositories;
using PluginLoader;
using Repositories;
using Service;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

//config shit
builder.Configuration.AddIniFile("settings.ini");
var webConfig = builder.Configuration.GetSection("web").Get<Configuration.WebConfig>();
if (webConfig is not null)
    Configuration.ConfigurationManager.WebConfig = webConfig;

var dbConfig = builder.Configuration.GetSection("database").Get<Configuration.DbConfig>();
if (dbConfig is not null)
    Configuration.ConfigurationManager.DbConfig = dbConfig;

var pluginsConfig = builder.Configuration.GetSection("plugins").Get<Configuration.PluginsConfig>();
if (pluginsConfig is not null)
    Configuration.ConfigurationManager.PluginsConfig = pluginsConfig;

//add logger for DI
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllersWithViews();

// Add services for DI 
builder.Services.AddTransient<IDbContext, DbContext>();
builder.Services.AddTransient<ILeaseRepository, LeaseRepository>();
builder.Services.AddTransient<IPluginRepository, PluginRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddSingleton<ILeaseService, LeaseService>();
builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
builder.Services.AddSingleton<IShutdownService, ShutdownService>();

builder.Services.AddAuthorization(options => options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Administrator")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = "/Authentication/Login";
            options.LogoutPath = "/Authentication/Logout";
        });
//builder.Services.AddMvc();
builder.Services.AddDistributedMemoryCache();

// add swagger stuff
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//fix a bug disabling sync call to streamreader
builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);

//build app -> create and inject services and controllers etc
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //tell the app to actually use the swagger stuff that was added
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
}

//https and auth stuff
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//subscribe all topics at startup
app.Lifetime.ApplicationStarted.Register(() =>
{
#if !DEBUG
    //do this in the background as to not block the api startup
    _ = Task.Factory.StartNew(() =>
    {
        var service = app.Services.GetRequiredService<ISubscriptionService>();
        service.SubscribeAll();
    });
#endif
});

//block SIGINT and unsubscribe all subscribed leases
//this doesnt work in docker (docker stop sends SIGTERM) - if someone finds a way to graceful shutdown in docker that keeps the api up until all unsubs came back lmk
Console.CancelKeyPress += (_, e) =>
{
    //cancel event -> dont stop the app
    e.Cancel = true;

    Console.WriteLine("Graceful shutdown -> unsubscribing all Leases.");
    var service = app.Services.GetRequiredService<IShutdownService>();
    service.Shutdown();
    //shutdown app after unsubs are done
    app.Lifetime.StopApplication();
};

//run... duh
app.Run();
