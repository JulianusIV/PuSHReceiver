using Data.JSONObjects;
using DefaultPlugins.DiscordPublisher;
using Plugins.Interfaces;
using System.Text.Json;

namespace DefaultPluginsTest
{
    [TestClass]
    public class DiscordPublisherUT
    {
        private IPublisherPlugin? _plugin;

        [TestInitialize]
        public void Init()
        {
            _plugin = new DiscordPublisherPlugin();
            _plugin.Init();
        }

        [TestMethod]
        public async Task DataSub_Publish_NoExceptionSuccess()
        {
            if (_plugin is null)
                Assert.Fail("Plugin failed to construct!");

            var publisherData = new
            {
                WebhookURL = Environment.GetEnvironmentVariable("DISCORD_TEST_WEBHOOK_URL"),
                PubName = "TestName",
                PubPfp = @"https://cdn.discordapp.com/attachments/512370308976607250/1009586807396106340/unknown.png",
                PubText = "Testing"
            };

            var data = new DataSub()
            {
                TopicID = 1,
                PublisherData = JsonSerializer.Serialize(publisherData)
            };

            var user = "TestUser";
            var itemUrl = @"https://www.youtube.com/";

            try
            {
                await _plugin.PublishAsync(data, user, itemUrl);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
