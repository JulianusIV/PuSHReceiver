using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubHubBubReciever.Controllers;
using System;
using System.Threading.Tasks;

namespace PubSubHubBubReciever
{
    public class Startup
    {
        private static bool _subscribed = false;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);

            services.AddControllers().AddXmlSerializerFormatters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            lifetime.ApplicationStarted.Register(OnAppStarted);
            lifetime.ApplicationStopping.Register(OnAppStopping);
        }

        private void OnAppStarted()
        {
#if !DEBUG
            if (!_subscribed)
            {
                _subscribed = true;
                SubscriptionHandler.SubscribeAll();
            }
#endif
        }

        private void OnAppStopping()
        {
            SubscriptionHandler.UnsubscribeAll();

            Console.WriteLine("Getting cancellation Token and waiting for cancellation or 3 min.");
            var cancellationToken = FeedRecieverController.tokenSource.Token;
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(180));
            Console.WriteLine(cancellationToken.IsCancellationRequested ?
                "Timeout cancelled, unsubscribe successful, continuing graceful shutdown." :
                "Timeout, shutting down w/o or with partial unsubscribe.");
        }
    }
}
