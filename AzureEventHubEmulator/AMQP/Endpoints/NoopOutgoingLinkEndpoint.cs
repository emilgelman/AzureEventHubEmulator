using Amqp.Listener;

namespace AzureEventHubEmulator.AMQP.Endpoints;

internal sealed class NoopOutgoingLinkEndpoint : LinkEndpoint
{
    public override void OnFlow(FlowContext flowContext)
    {
    }

    public override void OnDisposition(DispositionContext dispositionContext) =>
        dispositionContext.Complete();
}