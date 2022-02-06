using PubSubHubBubReciever.JSONObjects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace PubSubHubBubReciever
{
    public class FeedSubscriber
    {
        public static Dictionary<long, Timer> LeaseTimers = new Dictionary<long, Timer>();

        public static async Task<bool> SubscribeAsync(DataSub topic, bool subscribe = true)
        {
            Console.WriteLine("Requesting new subscription");
            using var client = new HttpClient();
            using var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://pubsubhubbub.appspot.com/subscribe");
            request.Method = HttpMethod.Post;

            var formList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("hub.mode", subscribe ? "subscribe" : "unsubscribe"),
                new KeyValuePair<string, string>("hub.topic", topic.TopicURL),
                new KeyValuePair<string, string>("hub.callback", SubscriptionHandler.GetTopicCallback(topic.TopicID)),
                new KeyValuePair<string, string>("hub.verify", "sync"),
                new KeyValuePair<string, string>("hub.secret", topic.Secret),
                new KeyValuePair<string, string>("hub.verify_token", topic.Token)
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
            return result;
        }

        public static void AwaitLease(long topicId, int leaseTime)
        {
            Console.WriteLine($"Scheduling lease renewal for topic {topicId} in {leaseTime} seconds");
            if (!LeaseTimers.ContainsKey(topicId))
                LeaseTimers.Add(topicId, new Timer());

            LeaseTimers[topicId].Stop();
            LeaseTimers[topicId].Interval = TimeSpan.FromSeconds(leaseTime).TotalMilliseconds;
            LeaseTimers[topicId].AutoReset = false;
            LeaseTimers[topicId].Elapsed += async (sender, e) => await SubscribeAsync(SubscriptionHandler.GetTopic(topicId));
            LeaseTimers[topicId].Start();
        }
    }
}
