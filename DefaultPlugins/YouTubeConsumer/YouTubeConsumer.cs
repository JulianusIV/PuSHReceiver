using Models;
using Models.ApiCommunication;
using PluginLibrary.Interfaces;
using PluginLibrary.PluginRepositories;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace DefaultPlugins.YouTubeConsumer
{
    public class YouTubeConsumer : IConsumerPlugin
    {
        //set a plugin name for resolving
        public string Name => "Default_YoutubeConsumer";
        //DI
        public IPluginRepository? PluginRepository { get; set; }

        public Response HandleGet(Lease lease, Request request)
        {
            var response = new Response()
            {
                Challenge = true
            };

            //transform plugin data saved in lease object as json
            var data = lease.GetObjectFromConsumerString<DefaultYouTubeConsPluginData>();
            if (data is null)
            {
                response.ReturnStatus = HttpStatusCode.InternalServerError;
                return response;
            }

            //verify tokens match
            if (request.QueryParameters["hub.verify_token"] != data.Token)
            {
                response.ReturnStatus = HttpStatusCode.Unauthorized;
                return response;
            }

            //verify topics match
            if (request.QueryParameters["hub.topic"] != lease.TopicUrl)
            {
                response.ReturnStatus = HttpStatusCode.Conflict;
                return response;
            }

            //update lease object in db
            //TODO add this as methods on the lease object instead, and pass parameters
            switch (request.QueryParameters["hub.mode"])
            {
                case "subscribe":
                    int leaseTime = int.Parse(request.QueryParameters["hub.lease_seconds"]);
                    lease.LeaseTime = leaseTime;
                    lease.LastLease = DateTime.Now;
                    lease.Subscribed = true;
                    PluginRepository!.SaveData(lease);

                    response.ResponseBody = request.QueryParameters["hub.challenge"];
                    response.ReturnStatus = HttpStatusCode.OK;
                    return response;
                case "unsubscribe":
                    lease.LeaseTime = 0;
                    lease.LastLease = DateTime.MinValue;
                    lease.Subscribed = false;
                    PluginRepository!.SaveData(lease);

                    response.ResponseBody = request.QueryParameters["hub.challenge"];
                    response.ReturnStatus = HttpStatusCode.OK;
                    return response;
                default:
                    response.ReturnStatus = HttpStatusCode.BadRequest;
                    return response;
            }
        }

        public Response HandlePost(Lease lease, Request request)
        {
            var response = new Response()
            {
                Challenge = false
            };

            //make sure plugin is subscribed
            if (!lease.Subscribed)
            {
                response.ReturnStatus = HttpStatusCode.PreconditionFailed;
                return response;
            }

            //check request body for content
            if (request.Body is null)
            {
                response.ReturnStatus = HttpStatusCode.BadRequest;
                return response;
            }

            //transform plugin data saved in lease object as json
            var data = lease.GetObjectFromConsumerString<DefaultYouTubeConsPluginData>();
            if (data is null)
            {
                response.ReturnStatus = HttpStatusCode.InternalServerError;
                return response;
            }

            //get hash from request header
            var headerHash = request.Headers["X-Hub-Signature"].ToString().Replace("sha1=", "");
            //get bytes from request body
            byte[] bytes = Encoding.UTF8.GetBytes(request.Body);
            //create hashing generator
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(data.Secret));
            if (hmac is null)
            {
                response.ReturnStatus = HttpStatusCode.InternalServerError;
                return response;
            }
            //create hash
            var hash = hmac.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
            //compare hashes
            if (headerHash != hashString)
            {
                response.ReturnStatus = HttpStatusCode.Unauthorized;
                return response;
            }
            //deserialize request body
            XmlSerializer serializer = new(typeof(feed));
            using StringReader stringReader = new(request.Body);
            feed? xml = (feed?)serializer.Deserialize(stringReader);

            if (xml is null)
            {
                response.ReturnStatus = HttpStatusCode.InternalServerError;
                return response;
            }
            //set properties to pass to publisher
            response.ItemUrl = xml.entry.link.href;
            response.Username = xml.entry.author.name;
            response.ReturnStatus = HttpStatusCode.OK;

            return response;
        }

        //no action needed on init
        public Task InitAsync()
            => Task.CompletedTask;

        public async Task<bool> SubscribeAsync(Lease lease, bool subscribe = true)
        {
            //transform plugin data saved in lease object as json
            var data = lease.GetObjectFromConsumerString<DefaultYouTubeConsPluginData>();
            if (data is null)
                return false;

            using var client = new HttpClient();
            using var request = new HttpRequestMessage();

            request.RequestUri = new(lease.HubUrl);
            request.Method = HttpMethod.Post;
            //set query parameters
            var formList = new Dictionary<string, string>()
            {
                { "hub.mode", subscribe ? "subscribe" : "unsubscribe" },
                { "hub.topic", lease.TopicUrl },
                { "hub.callback", lease.GetCallbackUrl() },
                { "hub.verify", "sync" },
                { "hub.secret", data.Secret },
                { "hub.verify_token", data.Token }
            };
            request.Content = new FormUrlEncodedContent(formList);

            var response = await client.SendAsync(request);
            var result = response.StatusCode == HttpStatusCode.NoContent;
            if (!result)
            {
                //TODO log failure
            }
            return result;
        }
    }
}
