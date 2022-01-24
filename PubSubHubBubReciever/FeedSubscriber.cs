using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PubSubHubBubReciever
{
    public class FeedSubscriber
    {
        public static bool IsSubscribed = false;

        public static async Task<bool> SubscribeAsync(bool subscribe = true)
        {
            Console.WriteLine("Requesting new subscription");
            using var client = new HttpClient();
            using var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://pubsubhubbub.appspot.com/subscribe");
            request.Method = HttpMethod.Post;

            var formList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("hub.mode", subscribe ? "subscribe" : "unsubscribe"),
                new KeyValuePair<string, string>("hub.topic", Environment.GetEnvironmentVariable(EnvVars.TOPIC.ToString())),
                new KeyValuePair<string, string>("hub.callback", Environment.GetEnvironmentVariable(EnvVars.CALLBACK_URL.ToString())),
                new KeyValuePair<string, string>("hub.verify", "sync"),
                new KeyValuePair<string, string>("hub.secret", Environment.GetEnvironmentVariable(EnvVars.HUB_SECRET.ToString())),
                new KeyValuePair<string, string>("hub.verify_token", Environment.GetEnvironmentVariable(EnvVars.HUB_TOKEN.ToString()))
            };
            request.Content = new FormUrlEncodedContent(formList);

            var response = await client.SendAsync(request);
            var result = response.StatusCode == HttpStatusCode.NoContent;
            if (!result)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(response.StatusCode + " - " + response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
                IsSubscribed = true;
            return result;
        }

        public static void AwaitLease(int leaseTime)
        {
            Console.WriteLine($"Scheduling lease renewal in {leaseTime} seconds");
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(leaseTime));
                await SubscribeAsync();
            });
        }
    }
}
