using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Amqp.Types;
using AzureEventHubEmulator.AMQP.Endpoints;
using AzureEventHubEmulator.Entities;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.AMQP.Processors;

public class LinkProcessor : ILinkProcessor
{
    private readonly ILogger _logger;
    private readonly ITopicRegistry _topicRegistry;

    public LinkProcessor(ILogger<LinkProcessor> logger, ITopicRegistry topicRegistry)
    {
        _logger = logger;
        _topicRegistry = topicRegistry;
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
        {
            AttachIncomingLink(attachContext, (Target)attachContext.Attach.Target);
        }
        else
        {
            AttachOutgoingLink(attachContext, (Source)attachContext.Attach.Source);
        }
    }

    private void AttachIncomingLink(AttachContext attachContext, Target target)
    {
        var topic = _topicRegistry.Find(target.Address);
        if (topic == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = "Topic not found." });
            _logger.LogError($"Could not attach incoming link to non-existing topic '{target.Address}'.");
            return;
        }

        var incomingLinkEndpoint = new IncomingLinkEndpoint(topic);
        attachContext.Complete(incomingLinkEndpoint, 300);
        _logger.LogInformation($"Attached incoming link to entity '{target.Address}'.");
    }

    private void AttachOutgoingLink(AttachContext attachContext, Source source)
    {
        var epoch = attachContext.Attach.Properties[new Symbol("com.microsoft:epoch")];
        if (epoch == null)
        {
            attachContext.Complete(new NoopOutgoingLinkEndpoint(), 0);
            return;
        }

        var topicName = "/" + source.Address.Split("/")[1];
        var topic = _topicRegistry.Find(topicName);
        if (topic == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = "Topic not found." });
            _logger.LogError($"Could not attach outgoing link to non-existing topic '{topicName}'.");
            return;
        }

        var outgoingLinkEndpoint = new OutgoingLinkEndpoint(_logger, topic.Reader(), attachContext.Link);
        attachContext.Complete(outgoingLinkEndpoint, 0);
        _logger.LogInformation($"Attached outgoing link to entity '{source.Address}'.");
    }
}