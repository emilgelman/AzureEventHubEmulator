using Amqp;
using Amqp.Listener;

namespace AzureEventHubEmulator.Entities;

internal interface IDeliveryQueue
{
    void Enqueue(Delivery delivery);

    Message Dequeue(CancellationToken cancellationToken);

    void Process(MessageContext messageContext);
}