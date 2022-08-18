using Data.JSONObjects;
using Microsoft.AspNetCore.Mvc;
using Plugins.Interfaces;
using Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PubSubHubBubReciever.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManageController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicUrl")] string topicUrl,
            [FromQuery(Name = "Consumer")] string consumerName,
            [FromQuery(Name = "Publisher")] string publisherName,
            [FromQuery(Name = "HubUrl")] string hubUrl)
        {
            var service = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>();

            if (!service.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (string.IsNullOrWhiteSpace(topicUrl))
                return StatusCode(400);

            if (string.IsNullOrWhiteSpace(topicUrl))
                return StatusCode(400);

            var id = ulong.Parse(new Random().Next(10000000, 100000000).ToString() + DateTimeOffset.Now.ToUnixTimeSeconds().ToString());

            consumerName = consumerName is null ? "Default_YouTubeConsumer" : consumerName;
            publisherName = publisherName is null ? "Default_DiscordPublisher" : publisherName;

            using var sr = new StreamReader(Request.Body);
            var bodyString = await sr.ReadToEndAsync();
            var infos = bodyString.Split(Environment.NewLine);

            var consumer = Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(consumerName);

            DataSub dataSub = new()
            {
                TopicID = id,
                TopicURL = topicUrl,
                HubURL = hubUrl,

                FeedConsumer = consumerName,
                FeedPublisher = publisherName,
                ConsumerData = consumer.AddSubscription(id, infos),
                PublisherData = Runtime.Instance.PluginLoader.ResolvePlugin<IPublisherPlugin>(publisherName).AddSubscription(id, infos),

                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            if (!service.AddTopic(dataSub))
                return StatusCode(500);

            if (!await consumer.SubscribeAsync(dataSub))
            {
                service.DeleteTopic(dataSub);
                return StatusCode(500);
            }
            dataSub.Subscribed = true;
            Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>().Save();

            return Ok(dataSub);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] ulong topicId)
        {
            var service = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>();

            if (!service.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (service.GetDataSub(topicId) is not DataSub dataSub)
                return StatusCode(404);

            if (dataSub.Subscribed)
                if (!await Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(dataSub.FeedConsumer).SubscribeAsync(dataSub, false))
                    return StatusCode(500);

            if (!service.DeleteTopic(dataSub))
                return StatusCode(500);

            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] ulong topicId,
            [FromQuery(Name = "topicUrl")] string topicUrl,
            [FromQuery(Name = "Consumer")] string consumer,
            [FromQuery(Name = "Publisher")] string publisher,
            [FromQuery(Name = "HubUrl")] string hubUrl)
        {
            var service = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>();

            if (!service.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (service.GetDataSub(topicId) is not DataSub dataSub)
                return StatusCode(404);

            await Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(dataSub.FeedConsumer).SubscribeAsync(dataSub, false);

            using var sr = new StreamReader(Request.Body);
            var bodyString = await sr.ReadToEndAsync();
            var infos = bodyString.Split(Environment.NewLine);

            var consumerPlugin = Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(consumer ?? dataSub.FeedConsumer);
            var publisherPlugin = Runtime.Instance.PluginLoader.ResolvePlugin<IPublisherPlugin>(publisher ?? dataSub.FeedPublisher);

            DataSub topic = new()
            {
                TopicID = topicId,
                TopicURL = topicUrl ?? dataSub.TopicURL,
                HubURL = hubUrl ?? dataSub.HubURL,

                FeedConsumer = consumer ?? dataSub.FeedConsumer,
                FeedPublisher = publisher ?? dataSub.FeedPublisher,
                ConsumerData = consumer is null ? consumerPlugin.UpdateSubscription(topicId, dataSub.ConsumerData, infos) : consumerPlugin.AddSubscription(topicId, infos),
                PublisherData = publisher is null ? publisherPlugin.UpdateSubscription(topicId, dataSub.PublisherData, infos) : consumerPlugin.AddSubscription(topicId, infos),

                LastLease = DateTime.MinValue,
                LeaseTime = 0,
                Subscribed = false
            };

            if (!service.UpdateTopic(topic))
                return StatusCode(500);

            if (!await consumerPlugin.SubscribeAsync(topic))
                return StatusCode(500);
            return Ok();
        }

        [HttpPost]
        [Route("ToggleSub")]
        public async Task<IActionResult> ToggleSubscription([FromQuery(Name = "adminToken")] string adminToken,
            [FromQuery(Name = "topicId")] ulong topicId)
        {
            var service = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>();

            if (!service.VerifyAdminToken(adminToken))
                return StatusCode(401);

            if (service.GetDataSub(topicId) is not DataSub dataSub)
                return StatusCode(404);

            if (!await Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(dataSub.FeedConsumer).SubscribeAsync(dataSub, !dataSub.Subscribed))
                return StatusCode(500);

            return Ok("Status changed to: " + dataSub.Subscribed);
        }
    }
}
