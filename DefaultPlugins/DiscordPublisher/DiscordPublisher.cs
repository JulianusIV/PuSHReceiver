using Microsoft.Extensions.Logging;
using Models;
using PluginLibrary.Interfaces;
using PluginLibrary.PluginRepositories;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DefaultPlugins.DiscordPublisher
{
    public class DiscordPublisher : IPublisherPlugin
    {
        //cache to avoid duplicates
        private Dictionary<int, Queue<string>> _lastPublishes = new();
        //plugin name for resolving plugin
        public string Name => "Default_DiscordPublisher";
        //DI
        public IPluginRepository? PluginRepository { get; set; }
        public ILogger? Logger { get; set; }

        public async Task InitAsync()
        {
            //read cache from file, if file is present
            if (!File.Exists("Plugins/DiscordPublisherUrlCache.json"))
                return;
            var jsonString = await File.ReadAllTextAsync("Plugins/DiscordPublisherUrlCache.json");
            var jsonObject = JsonSerializer.Deserialize<Dictionary<int, Queue<string>>>(jsonString);
            if (jsonObject is null)
                return;
            _lastPublishes = jsonObject;
        }

        public async Task PublishAsync(Lease lease, string user, string itemUrl, string eventId, params string[] args)
        {
            //check cache and populate if new
            if (_lastPublishes.ContainsKey(lease.Id))
            {
                if (_lastPublishes[lease.Id].Contains(eventId))
                    return;
                else
                {
                    _lastPublishes[lease.Id].Enqueue(eventId);
                    if (_lastPublishes[lease.Id].Count > 12)
                        _lastPublishes[lease.Id].Dequeue();
                }
            }
            else
            {
                var queue = new Queue<string>();
                queue.Enqueue(eventId);
                _lastPublishes.Add(lease.Id, queue);
            }

            //transform plugin data stored in Lease object as json
            var pluginData = lease.GetObjectFromPublisherString<DefaultDiscordPubPluginData>();
            if (pluginData is null)
                return;
            //create request body
            string content = $"{{\"username\": \"{pluginData.PubName}\", " +
                $"\"content\": \"{Regex.Unescape(pluginData.PubText) + " " + itemUrl}\", " +
                $"\"avatar_url\": \"{pluginData.PubPfp}\"}}";

            //send request
            using var client = new HttpClient();
            await client.PostAsync(pluginData.WebhookUrl, new StringContent(content, Encoding.UTF8, "application/json"));

            lock (_lastPublishes)
            {
                //save cache
                if (!Directory.Exists("Plugins"))
                    Directory.CreateDirectory("Plugins");
                var jsonString = JsonSerializer.Serialize(_lastPublishes);
                File.WriteAllText("Plugins/DiscordPublisherUrlCache.json", jsonString);
            }
        }
    }
}
