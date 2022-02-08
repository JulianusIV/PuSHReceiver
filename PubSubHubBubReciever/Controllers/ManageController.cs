using Microsoft.AspNetCore.Mvc;
using PubSubHubBubReciever.JSONObjects;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PubSubHubBubReciever.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManageController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicUrl")] string topicUrl,
            [FromQuery(Name = "webhookUrl")] string webhookUrl,
            [FromQuery(Name = "pubText")] string pubText,
            [FromQuery(Name = "pubPfp")] string pubPfp,
            [FromQuery(Name = "pubName")] string pubName)
        {
            if (!SubscriptionHandler.VerifyAdminToken(adminToken))
                return StatusCode(401);

            Random random = new Random();

            var id = long.Parse(random.Next(10000, 100000).ToString() + DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            var idBytes = BitConverter.GetBytes(id);
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
                PubText = pubText,
                PubProfilePic = pubPfp,
                PubName = pubName,
                WebhookURL = webhookUrl
            };

            LeaseSub lease = new LeaseSub()
            {
                TopicID = id,
                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            if (!SubscriptionHandler.AddTopic(topic, lease).GetAwaiter().GetResult())
                return StatusCode(500);

            return Ok(id.ToString() + Environment.NewLine + token + Environment.NewLine + secret);
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] long topicId)
        {
            if (!SubscriptionHandler.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (!SubscriptionHandler.TopicExists(topicId))
                return StatusCode(404);

            if (!SubscriptionHandler.RemoveTopic(topicId).GetAwaiter().GetResult())
                return StatusCode(500);

            return Ok();
        }

        [HttpPatch]
        public IActionResult Patch([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] long topicId,
            [FromQuery(Name = "topicUrl")] string topicUrl,
            [FromQuery(Name = "webhookUrl")] string webhookUrl,
            [FromQuery(Name = "pubText")] string pubText,
            [FromQuery(Name = "pubPfp")] string pubPfp,
            [FromQuery(Name = "pubName")] string pubName)
        {
            if (!SubscriptionHandler.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (!SubscriptionHandler.TopicExists(topicId))
                return StatusCode(404);

            var oldTopic = SubscriptionHandler.GetTopic(topicId);

            DataSub topic = new DataSub()
            {
                TopicID = topicId,
                TopicURL = topicUrl,
                Token = oldTopic.Token,
                Secret = oldTopic.Secret,
                PubText = pubText,
                PubProfilePic = pubPfp,
                PubName = pubName,
                WebhookURL = webhookUrl
            };

            LeaseSub lease = new LeaseSub()
            {
                TopicID = topicId,
                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            if (!SubscriptionHandler.UpdateTopic(topic, lease).GetAwaiter().GetResult())
                return StatusCode(500);

            return Ok();
        }
    }
}
