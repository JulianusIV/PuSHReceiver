//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;

//namespace PubSubHubBubReciever.Controllers
//{
//#pragma warning disable IDE0060
//    [Route("[controller]")]
//    [ApiController]
//    public class ManageController : ControllerBase
//    {
//        [HttpPost]
//        public async Task<IActionResult> Post([FromQuery(Name = "adminToken")] string adminToken,
//            [FromQuery(Name = "topicUrl")] string topicUrl,
//            [FromQuery(Name = "Parser")] string parserName,
//            [FromQuery(Name = "Publisher")] string publisherName)
//        {
//            await Task.Delay(1);
//            if (!dataService.VerifyAdminToken(adminToken))
//                return StatusCode(401);

//            if (string.IsNullOrWhiteSpace(topicUrl))
//                return StatusCode(400);

//            if (string.IsNullOrWhiteSpace(topicUrl))
//                return StatusCode(400);

//            var id = ulong.Parse(new Random().Next(10000000, 100000000).ToString() + DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
//            var idBytes = BitConverter.GetBytes(id);
//            var token = BitConverter.ToString(SHA256.Create().ComputeHash(idBytes)).Replace("-", "").ToLower();

//            var tokenBytes = Encoding.UTF8.GetBytes(token);

//            var secretBytes = idBytes.ToList();
//            secretBytes.AddRange(tokenBytes);

//            var secret = BitConverter.ToString(SHA256.Create().ComputeHash(secretBytes.ToArray())).Replace("-", "").ToLower();

//            DataSub dataSub = new DataSub()
//            {
//                TopicID = id,
//                TopicURL = topicUrl,
//                Token = token,
//                Secret = secret,
//                FeedParser = parserName is null ? "YouTubeXmlParser" : parserName,
//                FeedPublisher = publisherName is null ? "DiscordWebhookPublisher" : publisherName
//            };

//            LeaseSub leaseSub = new LeaseSub()
//            {
//                TopicID = id,
//                LastLease = DateTime.MinValue,
//                LeaseTime = 0,
//                Subscribed = false
//            };

//            using var sr = new StreamReader(HttpContext.Request.Body);
//            var bodyString = await sr.ReadToEndAsync();

//            var parser = PluginManager.Instance.ResolveParserPlugin(dataSub.FeedParser);
//            if (parser is null)
//                return StatusCode(426, $"No parser plugin with name {dataSub.FeedParser} is registered");
//            var publisher = PluginManager.Instance.ResolvePublishPlugin(dataSub.FeedPublisher);
//            if (publisher is null)
//                return StatusCode(426, $"No publisher plugin with name {dataSub.FeedParser} is registered");

//            dataSub.ParserData = parser.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));
//            dataSub.PublisherData = publisher.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));

//            if (!dataService.AddTopic(dataSub, leaseSub))
//                return StatusCode(500);

//            if (!await subscriptionService.SubscribeAsync(dataSub))
//            {
//                dataService.DeleteTopic(dataSub, leaseSub);
//                return StatusCode(500);
//            }

//            return Ok(id.ToString() + Environment.NewLine + token + Environment.NewLine + secret);
//        }

//        [HttpDelete]
//        public async Task<IActionResult> Delete([FromQuery(Name = "adminToken")] string adminToken,
//            [FromQuery(Name = "topicId")] ulong topicId)
//        {
//            await Task.Delay(1);
//            if (!dataService.VerifyAdminToken(adminToken))
//                return StatusCode(401);

//            if (!(dataService.GetLeaseSub(topicId) is LeaseSub leaseSub))
//                return StatusCode(404);
//            var dataSub = dataService.GetDataSub(topicId);

//            if (leaseSub.Subscribed)
//                if (!await subscriptionService.SubscribeAsync(dataSub, false))
//                    return StatusCode(500);

//            if (!dataService.DeleteTopic(dataSub, leaseSub))
//                return StatusCode(500);

//            return Ok();
//        }

//        [HttpPatch]
//        public async Task<IActionResult> Patch([FromQuery(Name = "adminToken")] string adminToken,
//            [FromQuery(Name = "topicId")] ulong topicId,
//            [FromQuery(Name = "topicUrl")] string topicUrl,
//            [FromQuery(Name = "Parser")] string parser,
//            [FromQuery(Name = "Publisher")] string publisher)
//        {
//            await Task.Delay(1);
//            if (!dataService.VerifyAdminToken(adminToken))
//                return StatusCode(401);

//            if (!(dataService.GetLeaseSub(topicId) is LeaseSub leaseSub))
//                return StatusCode(404);
//            var dataSub = dataService.GetDataSub(topicId);

//            DataSub topic = new DataSub()
//            {
//                TopicID = topicId,
//                TopicURL = topicUrl ?? dataSub.TopicURL,
//                Token = dataSub.Token,
//                Secret = dataSub.Secret,
//                FeedParser = parser ?? dataSub.FeedParser,
//                FeedPublisher = publisher ?? dataSub.FeedPublisher
//            };

//            LeaseSub lease = new LeaseSub()
//            {
//                TopicID = topicId,
//                LastLease = DateTime.MinValue,
//                LeaseTime = 0,
//                Subscribed = false
//            };

//            using var sr = new StreamReader(HttpContext.Request.Body);
//            var bodyString = await sr.ReadToEndAsync();

//            var parserPlugin = PluginManager.Instance.ResolveParserPlugin(topic.FeedParser);
//            if (parserPlugin is null)
//                return StatusCode(426, $"No parser plugin with name {topic.FeedParser} is registered");
//            var publisherPlugin = PluginManager.Instance.ResolvePublishPlugin(topic.FeedPublisher);
//            if (publisherPlugin is null)
//                return StatusCode(426, $"No publisher plugin with name {topic.FeedPublisher} is registered");

//            dataSub.ParserData = parserPlugin.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));
//            dataSub.PublisherData = publisherPlugin.TopicAdded(dataSub, bodyString.Split(Environment.NewLine));

//            if (leaseSub.Subscribed)
//                if (!await subscriptionService.SubscribeAsync(dataSub, false))
//                    return StatusCode(500);

//            if (!dataService.UpdateTopic(topic, lease))
//                return StatusCode(500);

//            if (leaseSub.Subscribed)
//            {
//                if (!await subscriptionService.SubscribeAsync(topic))
//                {
//                    dataService.UpdateTopic(dataSub, leaseSub);
//                    return StatusCode(500);
//                }
//            }
//            else
//                return Ok("Topic is currently not subscribed, inputs have not been verified!");

//            return Ok();
//        }

//        [HttpPost]
//        [Route("ToggleSub")]
//        public async Task<IActionResult> ToggleSubscription([FromQuery(Name = "adminToken")] string adminToken,
//            [FromQuery(Name = "topicId")] ulong topicId)
//        {
//            await Task.Delay(1);
//            if (!dataService.VerifyAdminToken(adminToken))
//                return StatusCode(401);

//            if (!(dataService.GetLeaseSub(topicId) is LeaseSub leaseSub))
//                return StatusCode(404);
//            var dataSub = dataService.GetDataSub(topicId);

//            if (!await subscriptionService.SubscribeAsync(dataSub, !leaseSub.Subscribed))
//                return StatusCode(500);

//            return Ok("Status changed to: " + leaseSub.Subscribed);
//            return Ok();
//        }
//    }
//#pragma warning restore IDE0060
//}
