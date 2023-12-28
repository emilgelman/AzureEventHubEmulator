using Amqp;
using Amqp.Types;

namespace AzureEventHubEmulator.AMQP;

internal static class AmqpExtensions
{
    private static readonly Symbol XOptSequenceNumber = "x-opt-sequence-number";

    internal static Message Clone(this Message message)
        => message == null
            ? null
            : Message.Decode(message.Encode());

    internal static Message AddSequenceNumber(this Message message, long sequence)
    {
        if (message != null)
        {
            if (message.MessageAnnotations == null)
                message.MessageAnnotations = new Amqp.Framing.MessageAnnotations();
            if (message.MessageAnnotations[XOptSequenceNumber] == null)
                message.MessageAnnotations[XOptSequenceNumber] = sequence;
        }

        return message;
    }
}