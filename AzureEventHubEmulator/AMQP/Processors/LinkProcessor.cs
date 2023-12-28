using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using AzureEventHubEmulator.AMQP.Endpoints;
using AzureEventHubEmulator.Entities;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.AMQP.Processors;

public class LinkProcessor : ILinkProcessor
{
    private readonly ILogger _logger;
    private readonly IEntityLookup _entityLookup;

    public LinkProcessor(ILogger<LinkProcessor> logger, IEntityLookup entityLookup)
    {
        _logger = logger;
        _entityLookup = entityLookup;
    }

    public void Process(AttachContext attachContext)
    {
        if (string.IsNullOrEmpty(attachContext.Attach.LinkName))
        {
            attachContext.Complete(new Error(ErrorCode.InvalidField) { Description = "Empty link name not allowed." });
            _logger.LogError($"Could not attach empty link to {GetType().Name}.");
            return;
        }


        if (attachContext.Link.Role)
            AttachIncomingLink(attachContext, (Target)attachContext.Attach.Target);
        else
            AttachOutgoingLink(attachContext, (Source)attachContext.Attach.Source);
    }

    private void AttachIncomingLink(AttachContext attachContext, Target target)
    {
        IEntity entity = _entityLookup.Find(target.Address);
        if (entity == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = "Entity not found." });
            _logger.LogError($"Could not attach incoming link to non-existing entity '{target.Address}'.");
            return;
        }


        var incomingLinkEndpoint = new IncomingLinkEndpoint(entity);
        attachContext.Complete(incomingLinkEndpoint, 300);
        _logger.LogInformation($"Attached incoming link to entity '{target.Address}'.");
    }

    private void AttachOutgoingLink(AttachContext attachContext, Source source)
    {
        IEntity entity = _entityLookup.Find(source.Address);
        if (entity == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = "Entity not found." });
            _logger.LogError($"Could not attach outgoing link to non-existing entity '{source.Address}'.");
            return;
        }

        DeliveryQueue queue = entity.DeliveryQueue;
        if (queue == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = "Queue not found." });
            _logger.LogError($"Could not attach outgoing link to non-existing queue '{source.Address}'.");
            return;
        }

        var outgoingLinkEndpoint = new OutgoingLinkEndpoint(queue);
        attachContext.Complete(outgoingLinkEndpoint, 0);
        _logger.LogInformation($"Attached outgoing link to entity '{source.Address}'.");
    }
}