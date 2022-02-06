using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace PubSubHubBubReciever.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedRecieverController : ControllerBase
    {
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();

        [HttpGet]
        [Route("{topicId}")]
        public IActionResult Get([FromRoute] long topicId,
            [FromQuery(Name = "hub.topic")] string hubTopic,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.lease_seconds")] int lease,
            [FromQuery(Name = "hub.verify_token")] string token)
        {
            Console.WriteLine($"Recieved HTTP-GET for topic {topicId} with params:\ntopic = {hubTopic}\nchallenge = {challenge}\nmode = {mode}\nlease = {lease}s\ntoken = {token}");

            if (!SubscriptionHandler.TopicExists(topicId))
                return StatusCode(404);

            if (!SubscriptionHandler.VerifyToken(topicId, token))
                return StatusCode(498);

            if (!SubscriptionHandler.VerifyTopicURL(topicId, hubTopic))
                return StatusCode(404);

            if (mode == "subscribe")
            {
                SubscriptionHandler.UpdateLeaseFile(topicId, true, lease);
                
                FeedSubscriber.AwaitLease(topicId, lease);

                var result = Content(challenge);
                result.StatusCode = 200;
                return result;
            }
            else if (mode == "unsubscribe")
            {
                SubscriptionHandler.UpdateLeaseFile(topicId, false);

                Console.WriteLine("Recieved unsubscribe request, sending back challenge.");

                var result = Content(challenge);
                result.StatusCode = 200;

                if (SubscriptionHandler.CountSubs() == 0)
                {
                    Console.WriteLine("No Feeds subscribed anymore, cancelling token!");
                    tokenSource.Cancel();
                }
                
                return result;
            }
            return StatusCode(405);
        }

        [HttpPost]
        [Route("{topicId}")]
        [Consumes("application/xml")]
        public IActionResult Post([FromRoute] long topicId)
        {
            if (!SubscriptionHandler.TopicExists(topicId))
                return StatusCode(404);

            if (!SubscriptionHandler.IsSubscribed(topicId))
                return StatusCode(412);

            var headerHash = HttpContext.Request.Headers["X-Hub-Signature"].ToString().Replace("sha1=", "");

            using var sr = new StreamReader(HttpContext.Request.Body);
            var bodyString = sr.ReadToEnd();
            byte[] bytes = Encoding.UTF8.GetBytes(bodyString);

            var hmac = HMAC.Create("HMACSHA1");
            string secret = SubscriptionHandler.GetSecret(topicId);
            hmac.Key = Encoding.UTF8.GetBytes(secret);
            var hash = hmac.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (headerHash != hashString)
            {
                Console.WriteLine("Incoming HTTP-POST with improper HMAC hash! Ignoring.");
                return StatusCode(401);
            }

            Console.WriteLine("Recieved HTTP-POST with body:\n" + bodyString);

            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            using StringReader stringReader = new StringReader(bodyString);
            feed xml = (feed)serializer.Deserialize(stringReader);

            if (xml.link is null)
            {
                Console.WriteLine("Incoming HTTP-POST with improper xml body! Ignoring.");
                return StatusCode(422);
            }

            if (!SubscriptionHandler.VerifyTopicURL(topicId, xml.link.Single(x => x.rel == "self").href))
                return StatusCode(404);

            Publisher.PublishToDiscord(SubscriptionHandler.GetTopic(topicId), xml.entry.link.href);

            return new OkResult();
        }
    }
}
