using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubHubBubReciever
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            foreach (var line in File.ReadAllLines("settings.env"))
            {
                Environment.SetEnvironmentVariable(line[..line.IndexOf('=')], line[(line.IndexOf('=') + 1)..]);
            }
#endif
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public enum EnvVars
    {
        TOPIC,
        CALLBACK_URL,
        USERNAME,
        WEBHOOK_URL,
        PUBLISH_TEXT,
        HUB_TOKEN,
        HUB_SECRET,
        HOOK_PFP
    }
}
