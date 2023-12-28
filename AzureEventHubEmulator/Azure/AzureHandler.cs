using Amqp.Framing;
using Amqp.Handler;

namespace AzureEventHubEmulator.Azure;

public class AzureHandler : IHandler
{
    public bool CanHandle(EventId id)
    {
        return id is EventId.SendDelivery or EventId.LinkLocalOpen;
    }

    public void Handle(Event protocolEvent)
    {
        switch (protocolEvent.Id)
        {
            case EventId.SendDelivery when protocolEvent.Context is IDelivery delivery:
                delivery.Tag = Guid.NewGuid().ToByteArray();
                break;
            case EventId.LinkLocalOpen when protocolEvent.Context is Attach attach:
                attach.MaxMessageSize = int.MaxValue;
                break;
        }
    }
}