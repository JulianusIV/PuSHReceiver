using Data.JSONObjects;
using Microsoft.AspNetCore.Http;
using Plugins.Interfaces;
using PubSubHubBubReciever;
using PubSubHubBubReciever.Controllers;
using Services;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace DefaultPlugins.YouTubeConsumer
{
    internal class YouTubeConsumerPlugin : IConsumerPlugin
    {
        public string Name => "Default_YouTubeConsumer";


        public Task InitAsync()
        {
            FeedRecieverController.OnGet += OnGetHandler;
            FeedRecieverController.OnPost += OnPostHandler;

            return Task.CompletedTask;
        }

        public async Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true)
        {
            var data = JsonSerializer.Deserialize<PluginData>(dataSub.ConsumerData);
            if (data is null)
                return false;


            using var client = new HttpClient();
            using var request = new HttpRequestMessage();

            request.RequestUri = new(dataSub.HubURL);
            request.Method = HttpMethod.Post;

            var formList = new Dictionary<string, string>()
            {
                { "hub.mode", subscribe ? "subscribe" : "unsubscribe" },
                { "hub.topic", dataSub.TopicURL },
                { "hub.callback", Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>().Data.CallbackURL + $"/{dataSub.TopicID}" },
                { "hub.verify", "sync" },
                { "hub.secret", data.Secret },
                { "hub.verify_token", data.Token }
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

        public string? AddSubscription(ulong id, params string[] _)
        {
            var idBytes = BitConverter.GetBytes(id);
            var token = BitConverter.ToString(SHA256.Create().ComputeHash(idBytes)).Replace("-", "").ToLower();

            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var secretBytes = idBytes.ToList();
            secretBytes.AddRange(tokenBytes);

            var secret = BitConverter.ToString(SHA256.Create().ComputeHash(secretBytes.ToArray())).Replace("-", "").ToLower();

            var ret = new PluginData()
            {
                Token = token,
                Secret = secret
            };
            return JsonSerializer.Serialize(ret);
        }

        public string? UpdateSubscription(ulong id, string oldData, params string[] additionalInfo)
            => oldData;

        private Response OnGetHandler(HttpRequest request, ulong topicId)
        {
            Response response = new()
            {
                Challenge = true
            };

            var service = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            var dataSub = service.Data.Subs.Find(x => x.TopicID == topicId);
            if (dataSub is null)
            {
                response.RetCode = HttpStatusCode.NotFound;
                return response;
            }

            var data = JsonSerializer.Deserialize<PluginData>(dataSub.ConsumerData);
            if (data is null)
            {
                response.RetCode = HttpStatusCode.Conflict;
                return response;
            }

            if (request.Query["hub.verify_token"] != data.Token)
            {
                response.RetCode = HttpStatusCode.Unauthorized;
                return response;
            }

            if (request.Query["hub.topic"] != dataSub.TopicURL)
            {
                response.RetCode = HttpStatusCode.NotFound;
                return response;
            }

            if (request.Query["hub.mode"] == "subscribe")
            {
                int leaseTime = int.Parse(request.Query["hub.lease_seconds"]);
                dataSub.LeaseTime = leaseTime;
                dataSub.LastLease = DateTime.Now;
                dataSub.Subscribed = true;
                service.Save();

                var leaseService = Runtime.Instance.ServiceLoader.ResolveService<ILeaseService>();
                leaseService.RegisterLease(dataSub, leaseTime);

                response.ReponseBody = request.Query["hub.challenge"];
                response.RetCode = HttpStatusCode.OK;
                return response;
            }
            else if (request.Query["hub.mode"] == "unsubscribe")
            {
                dataSub.LeaseTime = 0;
                dataSub.LastLease = DateTime.MinValue;
                dataSub.Subscribed = false;
                service.Save();

                response.ReponseBody = request.Query["hub.challenge"];
                response.RetCode = HttpStatusCode.OK;
                return response;
            }

            response.RetCode = HttpStatusCode.BadRequest;
            return response;
        }

        private Response OnPostHandler(HttpRequest request, ulong topicId)
        {
            Response response = new()
            {
                Challenge = false
            };

            var service = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            var dataSub = service.Data.Subs.Find(x => x.TopicID == topicId);
            if (dataSub is null)
            {
                response.RetCode = HttpStatusCode.NotFound;
                return response;
            }

            if (!dataSub.Subscribed)
            {
                response.RetCode = HttpStatusCode.PreconditionFailed;
                return response;
            }

            var data = JsonSerializer.Deserialize<PluginData>(dataSub.ConsumerData);
            if (data is null)
            {
                response.RetCode = HttpStatusCode.Conflict;
                return response;
            }

            var headerHash = request.Headers["X-Hub-Signature"].ToString().Replace("sha1=", "");

            using var sr = new StreamReader(request.Body);
            var bodyString = sr.ReadToEnd();
            byte[] bytes = Encoding.UTF8.GetBytes(bodyString);

            var hmac = HMAC.Create("HMACSHA1");
            string secret = data.Secret;

            if (hmac is null)
            {
                response.RetCode = HttpStatusCode.InternalServerError;
                return response;
            }

            hmac.Key = Encoding.UTF8.GetBytes(secret);
            var hash = hmac.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (headerHash != hashString)
            {
                response.RetCode = HttpStatusCode.Unauthorized;
                return response;
            }

            XmlSerializer serializer = new(typeof(feed));
            using StringReader stringReader = new(bodyString);
            feed? xml = (feed?)serializer.Deserialize(stringReader);

            if (xml is null)
            {
                response.RetCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.Item = dataSub;
            response.ItemUrl = xml.entry.link.href;
            response.User = xml.entry.author.name;
            response.RetCode = HttpStatusCode.OK;

            return response;
        }
    }
}
