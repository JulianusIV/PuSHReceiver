using DataLayer.JSONObject;
using Microsoft.AspNetCore.Mvc;
using Plugin;
using ServiceLayer.Interface;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PubSubHubBubReciever.Controllers
{
    [Route("[controller]")]
    [ApiController]
    internal class ManageController : ControllerBase
    {
        private readonly ITopicDataService dataService;
        private readonly ISubscriptionService subscriptionService;
        internal ManageController(ITopicDataService dataService, ISubscriptionService subscriptionService)
        {
            this.dataService = dataService;
            this.subscriptionService = subscriptionService;
        }

        [HttpPost]
        internal IActionResult Post([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicUrl")] string topicUrl,
            [FromQuery(Name = "Parser")] string parserName,
            [FromQuery(Name = "Publisher")] string publisherName)
        {
            if (!dataService.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (string.IsNullOrWhiteSpace(topicUrl))
                return StatusCode(400);

            if (string.IsNullOrWhiteSpace(topicUrl))
                return StatusCode(400);

            var id = Guid.NewGuid();
            var idBytes = id.ToByteArray();
            var token = BitConverter.ToString(SHA256.Create().ComputeHash(idBytes)).Replace("-", "").ToLower();

            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var secretBytes = idBytes.ToList();
            secretBytes.AddRange(tokenBytes);

            var secret = BitConverter.ToString(SHA256.Create().ComputeHash(secretBytes.ToArray())).Replace("-", "").ToLower();

            DataSub dataSub = new DataSub()
            {
                TopicID = id,
                TopicURL = topicUrl,
                Token = token,
                Secret = secret,
                FeedParser = parserName ?? "YouTubeXmlParser",
                FeedPublisher = publisherName ?? "DiscordWebhookPublisher"
            };

            LeaseSub leaseSub = new LeaseSub()
            {
                TopicID = id,
                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            using var sr = new StreamReader(HttpContext.Request.Body);
            var bodyString = sr.ReadToEnd();

            var parser = PluginManager.Instance.ResolveParserPlugin(parserName);
            if (parser is null)
                return StatusCode(426, $"No parser plugin with name {parserName} is registered");
            var publisher = PluginManager.Instance.ResolvePublishPlugin(publisherName);
            if (publisher is null)
                return StatusCode(426, $"No publisher plugin with name {publisherName} is registered");

            dataSub.ParserData = parser.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));
            dataSub.PublisherData = publisher.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));

            if (!dataService.AddTopic(dataSub, leaseSub))
                return StatusCode(500);

            if (!subscriptionService.SubscribeAsync(dataSub).GetAwaiter().GetResult())
            {
                dataService.DeleteTopic(dataSub, leaseSub);
                return StatusCode(500);
            }

            return Ok(id.ToString() + Environment.NewLine + token + Environment.NewLine + secret);
        }

        [HttpDelete]
        internal IActionResult Delete([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] Guid topicId)
        {
            if (!dataService.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (!(dataService.GetLeaseSub(topicId) is LeaseSub leaseSub))
                return StatusCode(404);
            var dataSub = dataService.GetDataSub(topicId);

            if (leaseSub.Subscribed)
                if (!subscriptionService.SubscribeAsync(dataSub, false).GetAwaiter().GetResult())
                    return StatusCode(500);

            if (!dataService.DeleteTopic(dataSub, leaseSub))
                return StatusCode(500);

            return Ok();
        }

        [HttpPatch]
        internal IActionResult Patch([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] Guid topicId,
            [FromQuery(Name = "topicUrl")] string topicUrl,
            [FromQuery(Name = "Parser")] string parser,
            [FromQuery(Name = "Publisher")] string publisher)
        {
            if (!dataService.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (!(dataService.GetLeaseSub(topicId) is LeaseSub leaseSub))
                return StatusCode(404);
            var dataSub = dataService.GetDataSub(topicId);

            DataSub topic = new DataSub()
            {
                TopicID = topicId,
                TopicURL = topicUrl ?? dataSub.TopicURL,
                Token = dataSub.Token,
                Secret = dataSub.Secret,
                FeedParser = parser ?? dataSub.FeedParser,
                FeedPublisher = publisher ?? dataSub.FeedPublisher
            };

            LeaseSub lease = new LeaseSub()
            {
                TopicID = topicId,
                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            using var sr = new StreamReader(HttpContext.Request.Body);
            var bodyString = sr.ReadToEnd();

            var parserPlugin = PluginManager.Instance.ResolveParserPlugin(topic.FeedParser);
            if (parserPlugin is null)
                return StatusCode(426, $"No parser plugin with name {topic.FeedParser} is registered");
            var publisherPlugin = PluginManager.Instance.ResolvePublishPlugin(topic.FeedPublisher);
            if (publisherPlugin is null)
                return StatusCode(426, $"No publisher plugin with name {topic.FeedPublisher} is registered");

            dataSub.ParserData = parserPlugin.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));
            dataSub.PublisherData = publisherPlugin.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));

            if (leaseSub.Subscribed)
                if (!subscriptionService.SubscribeAsync(dataSub, false).GetAwaiter().GetResult())
                    return StatusCode(500);

            if (!dataService.UpdateTopic(topic, lease))
                return StatusCode(500);

            if (leaseSub.Subscribed)
            {
                if (!subscriptionService.SubscribeAsync(topic).GetAwaiter().GetResult())
                {
                    dataService.UpdateTopic(dataSub, leaseSub);
                    return StatusCode(500);
                }
            }

            return Ok();
        }
    }
}
