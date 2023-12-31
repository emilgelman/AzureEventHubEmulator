using Amqp.Listener;
using AzureEventHubEmulator.Entities;

namespace AzureEventHubEmulator.AMQP.Endpoints;

internal sealed class IncomingLinkEndpoint : LinkEndpoint
{
    private readonly Topic _topic;

    internal IncomingLinkEndpoint(Topic topic)
    {
        _topic = topic;
    }

    public override void OnMessage(MessageContext messageContext)
    {
        _topic.PublishMessage(messageContext.Message.Clone());
        messageContext.Complete();
    }

    public override void OnFlow(FlowContext flowContext)
        => throw new NotSupportedException();

    public override void OnDisposition(DispositionContext dispositionContext)
        => throw new NotSupportedException();
}