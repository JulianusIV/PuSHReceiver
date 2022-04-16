using DataLayer.JSONObject;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLayer.Service
{
    public class SubscriptionService : ISubscriptionService
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
            Console.WriteLine(request.RequestUri);
            var formList = new Dictionary<string, string>()
            {
                { "hub.mode", subscribe ? "subscribe" : "unsubscribe" },
                { "hub.topic", dataSub.TopicURL },
                { "hub.callback", dataService.GetCallback(dataSub.TopicID) },
                { "hub.verify", "sync" },
                { "hub.secret", dataSub.Secret },
                { "hub.verify_token", dataSub.Token }
            };
            request.Content = new FormUrlEncodedContent(formList);


            var response = await client.SendAsync(request);
            var result = response.StatusCode == HttpStatusCode.NoContent;
            if (!result)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(response.StatusCode + " - " + await response.Content.ReadAsStringAsync());
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return result;
        }

        async void ISubscriptionService.UnsubscribeAll()
        {
            foreach (var topic in dataService.GetSubbedTopics())
                await ((ISubscriptionService)this).SubscribeAsync(topic, false);
        }
    }
}
