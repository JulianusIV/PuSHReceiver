using DefaultPlugins.TwitchConsumer.TwitchApiModels;
using Microsoft.Extensions.Logging;
using Models;
using Models.ApiCommunication;
using PluginLibrary.Interfaces;
using PluginLibrary.PluginRepositories;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DefaultPlugins.TwitchConsumer
{
    public class TwitchConsumer : IConsumerPlugin
    {
        private TwitchTokenResponse _token = new();

        public string Name => "Default_TwitchConsumer";

        public IPluginRepository? PluginRepository { get; set; }
        public ILogger? Logger { get; set; }

        public Response HandleGet(Lease lease, Request request) 
            => throw new NotImplementedException("This should not be called!");

        public Response HandlePost(Lease lease, Request request)
        {
            var data = lease.GetObjectFromConsumerString<DefaultTwitchConsPluginData>();
            if (data is null)
                return new Response() { ReturnStatus = HttpStatusCode.InternalServerError };

            if (request.Body is null)
                return new Response() { ReturnStatus = HttpStatusCode.BadRequest };

            var headerHash = request.Headers["Twitch-Eventsub-Message-Signature"].ToString().Replace("sha256=", "");

            byte[] bytes = Encoding.UTF8.GetBytes(request.Body);
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(data.Secret));
            if (hmac is null)
                return new Response() { ReturnStatus = HttpStatusCode.InternalServerError };

            var hash = hmac.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (headerHash != hashString)
                return new Response() { ReturnStatus = HttpStatusCode.Unauthorized };

            if (!request.Headers.ContainsKey("Twitch-Eventsub-Message-Type"))
                return new Response() { ReturnStatus = HttpStatusCode.Forbidden };

            var messageType = request.Headers["Twitch-Eventsub-Message-Type"];

            switch (messageType)
            {
                case "notification":
                    var notifPayload = JsonSerializer.Deserialize<TwitchNotificationJson>(request.Body);
                    if (notifPayload is null)
                        return new Response() { ReturnStatus = HttpStatusCode.InternalServerError };

                    return new Response()
                    {
                        ItemUrl = $"https://www.twitch.tv/{notifPayload.Event.BroadcasterUserName}",
                        ReturnStatus = HttpStatusCode.OK,
                        Username = notifPayload.Event.BroadcasterUserName
                    };
                case "webhook_callback_verification":
                    var verifyPayload = JsonSerializer.Deserialize<TwitchCallbackVerificationJson>(request.Body);
                    if (verifyPayload is null)
                        return new Response() { ReturnStatus = HttpStatusCode.InternalServerError };

                    lease.Subscribed = true;

                    PluginRepository!.SaveData(lease);
                    return new Response()
                    {
                        Challenge = true,
                        ResponseBody = verifyPayload.Challenge,
                        ReturnStatus = HttpStatusCode.OK
                    };
                case "revocation":
                    lease.Active = false;
                    lease.Subscribed = false;

                    PluginRepository!.SaveData(lease);
                    return new Response()
                    {
                        ReturnStatus = HttpStatusCode.OK
                    };
                default:
                    return new Response() { ReturnStatus = HttpStatusCode.Forbidden };
            }
        }

        public Task InitAsync()
            => Task.CompletedTask;

        public async Task<bool> SubscribeAsync(Lease lease, bool subscribe = true)
        {
            if (!subscribe) 
                return true;

            var data = lease.GetObjectFromConsumerString<DefaultTwitchConsPluginData>();
            if (data is null)
                return false;

            bool isSuccess;
            int trys = 0;
            do
            {
                using HttpClient client = new();

                HttpRequestMessage request = new(HttpMethod.Post, @"https://api.twitch.tv/helix/eventsub/subscriptions")
                {
                    Content = new StringContent($$"""
                         {"type":"stream.online","version":"1","condition":{"broadcaster_user_id":"{{lease.TopicUrl}}"},"transport":{"method":"webhook","callback":"{{lease.GetCallbackUrl()}}","secret":"{{data.Secret}}"} }
                         """, Encoding.UTF8, "application/json")
                };

                var token = await GetAccessToken(data, trys > 0);
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    string.Concat(token.TokenType[0].ToString().ToUpper(), token.TokenType.AsSpan(1)), 
                    token.AccessToken);
                request.Headers.Add("Client-Id", data.ClientId);

                var response = await client.SendAsync(request);

                isSuccess = response.IsSuccessStatusCode;
                trys++;

                if (!isSuccess)
                    Logger!.LogWarning("Request to Twitch failed with response content: {}\nOn try {}", await response.Content.ReadAsStringAsync(), trys);
            } while (!isSuccess && trys < 3);
            if (!isSuccess)
                Logger!.LogError("Request to twitch failed after 3 trys");
            return isSuccess;
        }

        private async Task<TwitchTokenResponse> GetAccessToken(DefaultTwitchConsPluginData data, bool useCache = true)
        {
            if (useCache && !string.IsNullOrEmpty(_token.AccessToken) && _token.GrantedAt + TimeSpan.FromSeconds(_token.ExpiresInSeconds) > DateTime.Now)
                return _token;

            using HttpClient client = new();

            HttpRequestMessage request = new(HttpMethod.Post, @"https://id.twitch.tv/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", data.ClientId },
                    { "client_secret", data.ClientSecret },
                    { "grant_type", "client_credentials" }
                })
            };

            var response = client.Send(request);
            var tokenResponse = JsonSerializer.Deserialize<TwitchTokenResponse>(await response.Content.ReadAsStreamAsync());
            if (tokenResponse != null)
            {
                tokenResponse.GrantedAt = DateTime.Now;
                _token = tokenResponse;
            }
            return _token;
        }
    }
}
