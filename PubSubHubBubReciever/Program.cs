using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Services;

namespace PubSubHubBubReciever
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var test = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>().AddTopic(new(), new());

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
