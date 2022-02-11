using DataLayer.JSONObject;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plugin;
using ServiceLayer.Interface;
using ServiceLayer.Service;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PubSubHubBubReciever.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedRecieverController : ControllerBase
    {
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly ITopicDataService dataService;

        public FeedRecieverController(ITopicDataService dataService)
        {
            this.dataService = dataService;
        }

        [HttpGet]
        [Route("{topicId}")]
        public IActionResult Get([FromRoute] Guid topicId,
            [FromQuery(Name = "hub.topic")] string hubTopic,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.lease_seconds")] int lease,
            [FromQuery(Name = "hub.verify_token")] string token)
        {
            Console.WriteLine($"Recieved HTTP-GET for topic {topicId} with params:\ntopic = {hubTopic}\nchallenge = {challenge}\nmode = {mode}\nlease = {lease}s\ntoken = {token}");

            if (!(dataService.GetDataSub(topicId) is DataSub dataSub))
                return StatusCode(404);

            if (token == dataSub.Token)
                return StatusCode(498);

            if (hubTopic == dataSub.TopicURL)
                return StatusCode(404);

            if (mode == "subscribe")
            {
                dataService.UpdateLease(topicId, true, lease);

                LeaseService.Instance.RegisterLease(dataSub, lease);

                return Ok(challenge);
            }
            else if (mode == "unsubscribe")
            {
                dataService.UpdateLease(topicId, false);

                Console.WriteLine("Recieved unsubscribe request, sending back challenge.");

                var result = Ok(challenge);

                if (dataService.CountSubbedTopics() == 0)
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
        public IActionResult Post([FromRoute] Guid topicId)
        {
            Console.WriteLine($"Incomping HTTP-POST for topic {topicId}");

            if (!(dataService.GetLeaseSub(topicId) is LeaseSub leaseSub))
                return StatusCode(404);
            var dataSub = dataService.GetDataSub(topicId);

            if (!leaseSub.Subscribed)
                return StatusCode(412);

            var headerHash = HttpContext.Request.Headers["X-Hub-Signature"].ToString().Replace("sha1=", "");

            using var sr = new StreamReader(HttpContext.Request.Body);
            var bodyString = sr.ReadToEnd();
            byte[] bytes = Encoding.UTF8.GetBytes(bodyString);

            var hmac = HMAC.Create("HMACSHA1");
            string secret = dataSub.Secret;
            hmac.Key = Encoding.UTF8.GetBytes(secret);
            var hash = hmac.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (headerHash != hashString)
            {
                Console.WriteLine("Incoming HTTP-POST with improper HMAC hash! Ignoring.");
                return StatusCode(401);
            }

            Console.WriteLine("Recieved HTTP-POST with body:\n" + bodyString);

            var parser = PluginManager.Instance.ResolveParserPlugin(dataSub.FeedParser);

            var feedItem = parser.FeedUpdate(bodyString);

            var publisher = PluginManager.Instance.ResolvePublishPlugin(dataSub.FeedPublisher);

            publisher.FeedUpdate(feedItem, dataSub);

            return Ok();
        }
    }
}
