using Contracts;
using Contracts.Repositories;
using Contracts.Service;
using Microsoft.AspNetCore.Mvc;
using Models.ApiCommunication;
using PluginLibrary.Interfaces;

namespace PubSubHubBubReciever.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecieverController : ControllerBase
    {
        private readonly ILogger<RecieverController> _logger;
        private readonly ILeaseRepository _leaseRepository;
        private readonly IPluginManager _pluginManager;
        private readonly IShutdownService _shutdownService;
        private readonly ILeaseService _leaseService;

        public RecieverController(ILogger<RecieverController> logger, 
            ILeaseRepository repository, 
            IPluginManager pluginManager, 
            IShutdownService shutdownService, 
            ILeaseService leaseService)
        {
            _logger = logger;
            _leaseRepository = repository;
            _pluginManager = pluginManager;
            _shutdownService = shutdownService;
            _leaseService = leaseService;
        }

        [HttpGet]
        [Route("{topicId}")]
        public IActionResult Get([FromRoute] int topicId)
        {
            _logger.LogDebug("Recieved HTTPGet in {Controller} with TopicId {TopicId}.", nameof(RecieverController), topicId);

            //fetch lease with given id from DB
            var topic = _leaseRepository.FindLease(topicId);
            if (topic is null)
            {
                _logger.LogDebug("Could not find TopicId {TopicId}, returning 404.", topicId);
                return NotFound();
            }

            if (!topic.Active)
            {
                _logger.LogDebug("TopicId {TopicId} is not running, returning 409.", topicId);
                return Conflict();
            }

            //get consumerplugin registered to this lease
            var consumer = _pluginManager.ResolvePlugin<IConsumerPlugin>(topic.Consumer);

            //build object for passing request to plugin
            var request = new Request();
            foreach (var param in Request.Query)
                request.QueryParameters.Add(param.Key, param.Value!);
            foreach (var header in Request.Headers)
                request.Headers.Add(header.Key, header.Value!);
            //pass request to plugin
            var response = consumer.HandleGet(topic, request);

            //if plugin states request was not a challenge pass to publisherplugin
            if (response.IsSuccessStatusCode && !response.Challenge)
                _pluginManager.ResolvePlugin<IPublisherPlugin>(topic.Publisher)
                    .PublishAsync(topic, response.Username, response.ItemUrl, response.Args);

            if (response.Challenge)
            {
                //if no leases are still subscribed send "ready for shutdown" to shutdownService
                if (_leaseRepository.CountSubscribedLeases() <= 0)
                    _shutdownService.TriggerAllSubsUnsubscribed();
                //if lease was renewed register a new timer in lease service to renew after it expires
                if (topic.LeaseTime > 0)
                    _leaseService.RegisterLease(topic);
            }

            //send back response
            ContentResult? result = null;
            if (response.ResponseBody is not null)
            {
                result = Content(response.ResponseBody);
                result.StatusCode = (int)response.ReturnStatus;
            }
            return result is null ? StatusCode((int)response.ReturnStatus) : result;
        }

        [HttpPost]
        [Route("{topicId}")]
        [Consumes("application/xml")]
        public IActionResult Post([FromRoute] int topicId)
        {
            _logger.LogDebug("Recieved HTTPPost in {Controller} with TopicId {TopicId}.", nameof(RecieverController), topicId);
            
            //fetch lease with given id from DB
            var topic = _leaseRepository.FindLease(topicId);
            if (topic is null)
            {
                _logger.LogDebug("Could not find TopicId {TopicId}, returning 404.", topicId);
                return NotFound();
            }

            if (!topic.Active)
            {
                _logger.LogDebug("TopicId {TopicId} is not running, returning 409.", topicId);
                return Conflict();
            }

            //get consumerplugin registered to this lease
            var consumer = _pluginManager.ResolvePlugin<IConsumerPlugin>(topic.Consumer);

            //build object for passing request to plugin
            var request = new Request();
            foreach (var param in Request.Query)
                request.QueryParameters.Add(param.Key, param.Value!);
            foreach (var header in Request.Headers)
                request.Headers.Add(header.Key, header.Value!);
            if (Request.Body is not null)
            {
                using var sr = new StreamReader(Request.Body);
                request.Body = sr.ReadToEnd();
            }
            //pass request to plugin
            var response = consumer.HandlePost(topic, request);

            //if plugin states request was not a challenge pass to publisherplugin
            if (response.IsSuccessStatusCode && !response.Challenge)
                _pluginManager.ResolvePlugin<IPublisherPlugin>(topic.Publisher)
                    .PublishAsync(topic, response.Username, response.ItemUrl, response.Args);

            if (response.Challenge)
            {
                //if no leases are still subscribed send "ready for shutdown" to shutdownService
                if (_leaseRepository.CountSubscribedLeases() <= 0)
                    _shutdownService.TriggerAllSubsUnsubscribed();
                //if lease was renewed register a new timer in lease service to renew after it expires
                if (topic.LeaseTime > 0)
                    _leaseService.RegisterLease(topic);
            }

            //send back response
            ContentResult? result = null;
            if (response.ResponseBody is not null)
            {
                result = Content(response.ResponseBody);
                result.StatusCode = (int)response.ReturnStatus;
            }
            return result is null ? StatusCode((int)response.ReturnStatus) : result;
        }
    }
}