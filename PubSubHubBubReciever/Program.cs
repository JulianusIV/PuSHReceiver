using DataAccessLayer.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Plugin;

namespace PubSubHubBubReciever
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TopicRepository.Load();
            PluginManager.Instance.Load();
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
