using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubHubBubReciever.Controllers;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubHubBubReciever
{
    public class Startup
    {
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
            _ = Task.Run(() =>
            {
                if (!FeedSubscriber.SubscribeAsync().GetAwaiter().GetResult())
                {
                    using WebClient webClient = new WebClient();
                    webClient.UploadValues(Environment.GetEnvironmentVariable("WEBHOOK_URL"), new NameValueCollection
                    {
                        { "username", Environment.GetEnvironmentVariable(EnvVars.USERNAME.ToString()) },
                        { "content", "Subscribe did not return 204/No Content!\nExiting!" }
                    });
                    Environment.Exit(0);
                }
            });
        }

        private void OnAppStopping()
        {
            if (FeedSubscriber.IsSubscribed)
            {
                FeedSubscriber.SubscribeAsync(false).GetAwaiter().GetResult();

                Console.WriteLine("Getting cancellation Token and waiting for Cancellation or 3 min.");
                var cancellationToken = FeedRecieverController.tokenSource.Token;
                cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(180));
                Console.WriteLine(cancellationToken.IsCancellationRequested ?
                    "Timeout cancelled, unsubscribe successful, continuing graceful shutdown." :
                    "Timeout, shutting down w/o unsubscribe.");
            }
        }
    }
}
