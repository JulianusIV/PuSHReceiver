using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
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
        public IActionResult Get([FromQuery(Name = "hub.topic")] string hubTopic,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.lease_seconds")] int lease,
            [FromQuery(Name = "hub.verify_token")] string token)
        {
            Console.WriteLine($"Recieved HTTP-GET with params:\ntopic = {hubTopic}\nchallenge = {challenge}\nmode = {mode}\nlease = {lease}s\ntoken = {token}");

            if (token != Environment.GetEnvironmentVariable(EnvVars.HUB_TOKEN.ToString()))
                return StatusCode(418);

            if (mode == "subscribe" && hubTopic == Environment.GetEnvironmentVariable(EnvVars.TOPIC.ToString()))
            {
                FeedSubscriber.AwaitLease(lease);

                var result = Content(challenge);
                result.StatusCode = 200;
                return result;
            }
            else if (mode == "unsubscribe" && hubTopic == Environment.GetEnvironmentVariable(EnvVars.TOPIC.ToString()))
            {
                Console.WriteLine("Recieved unsubscribe request, sending back challenge and cancelling token.");
                var result = Content(challenge);
                result.StatusCode = 200;
                tokenSource.Cancel();
                return result;
            }
            return StatusCode(404);
        }

        [HttpPost]
        [Consumes("application/xml")]
        public IActionResult Post()
        {
            if (!FeedSubscriber.IsSubscribed)
            {
                Console.WriteLine("Incoming HTTP-POST without subscription! Ignoring.");
                return StatusCode(418);
            }

            var headerHash = HttpContext.Request.Headers["X-Hub-Signature"].ToString().Replace("sha1=", "");

            using var sr = new StreamReader(HttpContext.Request.Body);
            var bodyString = sr.ReadToEnd();
            byte[] bytes = Encoding.UTF8.GetBytes(bodyString);

            var hmac = HMAC.Create("HMACSHA1");
            string secret = Environment.GetEnvironmentVariable(EnvVars.HUB_SECRET.ToString());
            hmac.Key = Encoding.UTF8.GetBytes(secret);
            var hash = hmac.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (headerHash != hashString)
            {
                Console.WriteLine("Incoming HTTP-POST with improper HMAC hash! Ignoring.");
                return StatusCode(418);
            }

            Console.WriteLine("Recieved HTTP-POST with body:\n" + bodyString);

            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            using StringReader stringReader = new StringReader(bodyString);
            feed xml = (feed)serializer.Deserialize(stringReader);

            if (xml.link is null)
            {
                Console.WriteLine("Incoming HTTP-POST with improper xml body! Ignoring.");
                return StatusCode(418);
            }

            Publisher.PublishToDiscord(xml.entry.link.href);

            return new OkResult();
        }
    }
}
