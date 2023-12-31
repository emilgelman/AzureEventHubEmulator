using System.Threading.Channels;
using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.AMQP.Endpoints;

internal sealed class OutgoingLinkEndpoint : LinkEndpoint
{
    private readonly ILogger _logger;
    private readonly ChannelReader<Message> _reader;
    private readonly ListenerLink _listenerLink;
    private CancellationTokenSource _flowTask;


    public OutgoingLinkEndpoint(ILogger logger, ChannelReader<Message> reader, ListenerLink attachContextLink)
    {
        _logger = logger;
        _reader = reader;
        _listenerLink = attachContextLink;
    }


    public override void OnFlow(FlowContext flowContext)
    {
        CancelFlowTask();
        _flowTask = new CancellationTokenSource();
        var cancellationToken = _flowTask.Token;
        Task.Run(() => SendMessages(flowContext, cancellationToken), cancellationToken);
    }

    private async Task SendMessages(FlowContext flowContext, CancellationToken cancellationToken)
    {
        var messages = flowContext.Messages;
        while (messages-- > 0)
        {
            try
            {
                // block until the channel has a message
                await _reader.WaitToReadAsync(cancellationToken);
                _reader.TryRead(out var message);
                if (message == null)
                {
                    return;
                }

                _listenerLink.SendMessage(message);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
            {
                _logger.LogDebug("Delivery queue cancelled.");
                return;
            }
        }
    }

    public override void OnDisposition(DispositionContext dispositionContext)
    {
        dispositionContext.Complete();
    }

    public override void OnMessage(MessageContext messageContext)
        => throw new NotSupportedException();

    public override void OnLinkClosed(ListenerLink link, Error error) => CancelFlowTask();

    private void CancelFlowTask()
    {
        _flowTask?.Cancel();
        _flowTask?.Dispose();
        _flowTask = null;
    }
}