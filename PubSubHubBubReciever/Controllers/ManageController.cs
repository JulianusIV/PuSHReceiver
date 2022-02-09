using Microsoft.AspNetCore.Mvc;
using PubSubHubBubReciever.DataService.Interface;
using PubSubHubBubReciever.JSONObject;
using PubSubHubBubReciever.Service.Interface;
using System;
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
            [FromQuery(Name = "webhookUrl")] string webhookUrl,
            [FromQuery(Name = "pubText")] string pubText,
            [FromQuery(Name = "pubPfp")] string pubPfp,
            [FromQuery(Name = "pubName")] string pubName,
            [FromQuery(Name = "Parser")] string parser,
            [FromQuery(Name = "Publisher")] string publisher)
        {
            if (!dataService.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (string.IsNullOrWhiteSpace(topicUrl))
                return StatusCode(400);

            if (string.IsNullOrWhiteSpace(webhookUrl))
                return StatusCode(400);

            if (string.IsNullOrWhiteSpace(topicUrl))
                return StatusCode(400);

            var id = new Guid();
            var idBytes = id.ToByteArray();
            var token = BitConverter.ToString(SHA256.Create().ComputeHash(idBytes)).Replace("-", "").ToLower();

            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var secretBytes = idBytes.ToList();
            secretBytes.AddRange(tokenBytes);

            var secret = BitConverter.ToString(SHA256.Create().ComputeHash(secretBytes.ToArray())).Replace("-", "").ToLower();

            DataSub topic = new DataSub()
            {
                TopicID = id,
                TopicURL = topicUrl,
                Token = token,
                Secret = secret,
                PubText = pubText ?? "@everyone new Upload" + Environment.NewLine,
                PubProfilePic = pubPfp ?? "https://cdn.discordapp.com/attachments/784535910175735838/935557046747676732/unknown.png",
                PubName = pubName ?? "PuSH",
                WebhookURL = webhookUrl,
                FeedParser = parser ?? "YouTubeXmlParser",
                FeedPublisher = publisher ?? "DiscordWebhookPublisher"
            };

            LeaseSub lease = new LeaseSub()
            {
                TopicID = id,
                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            if (!dataService.AddTopic(topic, lease))
                return StatusCode(500);

            if (!subscriptionService.SubscribeAsync(topic).GetAwaiter().GetResult())
            {
                dataService.DeleteTopic(topic, lease);
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
            [FromQuery(Name = "webhookUrl")] string webhookUrl,
            [FromQuery(Name = "pubText")] string pubText,
            [FromQuery(Name = "pubPfp")] string pubPfp,
            [FromQuery(Name = "pubName")] string pubName,
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
                PubText = pubText ?? dataSub.PubText,
                PubProfilePic = pubPfp ?? dataSub.PubProfilePic,
                PubName = pubName ?? dataSub.PubName,
                WebhookURL = webhookUrl ?? dataSub.WebhookURL,
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
