using Amqp;
using Amqp.Framing;
using Amqp.Listener;

namespace AzureEventHubEmulator.AMQP.Processors;

public class CbsRequestProcessor : IRequestProcessor
{
    public int Credit => 100;

    public void Process(RequestContext requestContext)
    {
        using Message message = GetResponseMessage(200, requestContext);
        requestContext.Complete(message);
    }

    private static Message GetResponseMessage(int responseCode, RequestContext requestContext)
        => new()
        {
            Properties = new Properties
            {
                CorrelationId = requestContext.Message.Properties.MessageId
            },
            ApplicationProperties = new ApplicationProperties
            {
                ["status-code"] = responseCode
            }
        };
}