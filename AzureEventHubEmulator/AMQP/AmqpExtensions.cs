using System.Text;
using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Amqp.Types;

namespace AzureEventHubEmulator.AMQP;

internal static class AmqpExtensions
{
    private static readonly Symbol ReceiverName = "com.microsoft:receiver-name";
    private static readonly Symbol Epoch = "com.microsoft:epoch";

    internal static string String(this Message message)
    {
        var body = message.Body;
        return body switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            AmqpValue amqpValue => amqpValue.Value?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }

    internal static string? ExtractTopicName(this RequestContext requestContext)
    {
        var message = requestContext.Message;
        var name = message.ApplicationProperties.Map["name"];
        return name?.ToString();
    }

    internal static string? ExtractReceiverName(this AttachContext attachContext)
    {
        var receiverName = attachContext.Attach.Properties[ReceiverName];
        return receiverName?.ToString();
    }

    internal static string? ExtractEpoch(this AttachContext attachContext)
    {
        var epoch = attachContext.Attach.Properties[Epoch];
        return epoch?.ToString();
    }

    internal static string ExtractTopicName(this Source source)
    {
        return "/" + source.Address.Split("/")[1];
    }
}