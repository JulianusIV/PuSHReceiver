using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Services;
using System;

namespace PubSubHubBubReciever
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        internal IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);

            services.AddControllers().AddXmlSerializerFormatters();

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "PubSubHubBubReciever", Version = "v1" }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PubSubHubBubReciever v1"));
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
            Runtime.Instance.ServiceLoader.ResolveService<ISubscriptionService>().SubscribeAll();
#endif
        }

        private void OnAppStopping()
        {
            Runtime.Instance.ServiceLoader.ResolveService<ISubscriptionService>().UnsubscribeAll();

            Console.WriteLine("Getting cancellation Token and waiting for cancellation or 3 min.");
            var cancellationToken = Runtime.Instance.TokenSource.Token;
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(3));
            Console.WriteLine(cancellationToken.IsCancellationRequested ?
                "Timeout cancelled, unsubscribe successful, continuing graceful shutdown." :
                "Timeout, shutting down w/o or with partial unsubscribe.");
        }
    }
}
