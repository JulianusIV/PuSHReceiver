using PubSubHubBubReciever.DataService.Interface;
using PubSubHubBubReciever.JSONObject;
using PubSubHubBubReciever.Service.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PubSubHubBubReciever.Service
{
    internal class SubscriptionService : ISubscriptionService
    {
        private readonly ITopicDataService dataService;

        public SubscriptionService(ITopicDataService dataService)
        {
            this.dataService = dataService;
        }

        async void ISubscriptionService.SubscribeAll()
        {
            (var dataSubs, var leaseSubs) = dataService.GetExpiredAndRunningSubs();

            foreach (var sub in dataSubs)
                await ((ISubscriptionService)this).SubscribeAsync(sub);

            foreach (var sub in leaseSubs)
                LeaseService.Instance.RegisterLease(dataService.GetDataSub(sub.TopicID),
                    (int)(sub.LastLease + TimeSpan.FromSeconds(sub.LeaseTime) - DateTime.Now).TotalSeconds);
        }

        async Task<bool> ISubscriptionService.SubscribeAsync(DataSub dataSub, bool subscribe)
        {
            Console.WriteLine("Requesting new subscription");
            using var client = new HttpClient();
            using var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://pubsubhubbub.appspot.com/subscribe");
            request.Method = HttpMethod.Post;

            var formList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("hub.mode", subscribe ? "subscribe" : "unsubscribe"),
                new KeyValuePair<string, string>("hub.topic", dataSub.TopicURL),
                new KeyValuePair<string, string>("hub.callback", dataService.GetCallback(dataSub.TopicID)),
                new KeyValuePair<string, string>("hub.verify", "sync"),
                new KeyValuePair<string, string>("hub.secret", dataSub.Secret),
                new KeyValuePair<string, string>("hub.verify_token", dataSub.Token)
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

        void ISubscriptionService.UnsubscribeAll()
        {
            throw new NotImplementedException();
        }
    }
}
