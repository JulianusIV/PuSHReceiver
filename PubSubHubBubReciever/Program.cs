using Contracts;
using Contracts.DbContext;
using Contracts.Repositorys;
using Contracts.Service;
using DataAccess;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PluginLibrary.PluginRepositories;
using PluginLoader;
using Repositories;
using Service;

var builder = WebApplication.CreateBuilder(args);

//add logger for DI
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.AddControllers();
// Add services for DI 
builder.Services.AddTransient<IDbContext, DbContext>();
builder.Services.AddTransient<ILeaseRepository, LeaseRepository>();
builder.Services.AddTransient<IPluginRepository, PluginRepository>();
builder.Services.AddTransient<ISubscriptionService, SubscriptionService>();
builder.Services.AddSingleton<ILeaseService, LeaseService>();
builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddSingleton<IShutdownService, ShutdownService>();

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

//https and auth stuff
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

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

//block shutdown and unsubscribe all subscribed leases
app.Lifetime.ApplicationStopped.Register(() =>
{
    var service = app.Services.GetRequiredService<IShutdownService>();
    service.Shutdown();
});

//run... duh
app.Run();
