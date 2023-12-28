using Amqp.Listener;
using AzureEventHubEmulator.Entities;

namespace AzureEventHubEmulator.AMQP.Endpoints;

internal sealed class IncomingLinkEndpoint : LinkEndpoint
{
    private readonly IEntity _entity;

    internal IncomingLinkEndpoint(IEntity entity)
    {
        _entity = entity;
    }

    public override void OnMessage(MessageContext messageContext)
    {
        _entity.Post(messageContext.Message.Clone());
        messageContext.Complete();
    }

    public override void OnFlow(FlowContext flowContext)
        => throw new NotSupportedException();

    public override void OnDisposition(DispositionContext dispositionContext)
        => throw new NotSupportedException();
}