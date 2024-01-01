using Amqp.Listener;
using AzureEventHubEmulator.Entities;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.AMQP.Endpoints;

internal sealed class IncomingLinkEndpoint : LinkEndpoint
{
    private readonly ILogger _logger;
    private readonly Topic _topic;

    internal IncomingLinkEndpoint(ILogger logger, Topic topic)
    {
        _logger = logger;
        _topic = topic;
    }

    public override void OnMessage(MessageContext messageContext)
    {
        var message = messageContext.Message;
        _logger.LogInformation("Received message: {message}", message.String());
        _topic.PublishMessage(message);
        messageContext.Complete();
    }

    public override void OnFlow(FlowContext flowContext)
        => throw new NotSupportedException();

    public override void OnDisposition(DispositionContext dispositionContext)
        => throw new NotSupportedException();
}