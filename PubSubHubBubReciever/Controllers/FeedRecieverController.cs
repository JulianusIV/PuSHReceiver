﻿using Microsoft.AspNetCore.Mvc;
using Plugins;
using System;
using System.IO;

namespace PubSubHubBubReciever.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedRecieverController : ControllerBase
    {
        [HttpGet]
        [Route("{topicId}")]
        public IActionResult Get([FromRoute] ulong topicId)
        {
            var request = new Request()
            {
                Headers = new(),
                QueryParameters = new(),
                Body = null,
            };


            foreach (var param in Request.Query)
                request.QueryParameters.Add(param.Key, param.Value);
            foreach (var header in Request.Headers)
                request.Headers.Add(header.Key, header.Value);
            if (Request.Body is not null)
            {
                using var sr = new StreamReader(Request.Body);
                request.Body = sr.ReadToEnd();
            }

            Console.WriteLine($"Incoming GET request to FeedReciever/{topicId}:");
            Console.WriteLine(request);

            var ret = ApiMethodSource.InvokeGet(request, topicId);

            if (!ret.Challenge && ret.IsSuccessStatusCode)
                ret.Item.Publish(ret.User, ret.ItemUrl, ret.Args);

            ContentResult result = null;
            if (ret.ReponseBody is not null)
            {
                result = Content(ret.ReponseBody);
                result.StatusCode = (int)ret.RetCode;
            }

            return result is null ? StatusCode((int)ret.RetCode) : result;
        }

        [HttpPost]
        [Route("{topicId}")]
        [Consumes("application/xml")]
        public IActionResult Post([FromRoute] ulong topicId)
        {
            var request = new Request()
            {
                Headers = new(),
                QueryParameters = new(),
                Body = null
            };

            foreach (var param in Request.Query)
                request.QueryParameters.Add(param.Key, param.Value);
            foreach (var header in Request.Headers)
                request.Headers.Add(header.Key, header.Value);
            if (Request.Body is not null)
            {
                using var sr = new StreamReader(Request.Body);
                request.Body = sr.ReadToEnd();
            }

            Console.WriteLine($"Incoming POST request to FeedReciever/{topicId}:");
            Console.WriteLine(request);

            var ret = ApiMethodSource.InvokePost(request, topicId);

            if (!ret.Challenge && ret.IsSuccessStatusCode)
                ret.Item.Publish(ret.User, ret.ItemUrl, ret.Args);

            ContentResult result = null;
            if (ret.ReponseBody is not null)
            {
                result = Content(ret.ReponseBody);
                result.StatusCode = (int)ret.RetCode;
            }

            return result is null ? StatusCode((int)ret.RetCode) : result;
        }
    }
}
