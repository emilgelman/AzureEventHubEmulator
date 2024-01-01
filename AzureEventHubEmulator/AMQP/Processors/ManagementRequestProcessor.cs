using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Amqp.Types;

namespace AzureEventHubEmulator.AMQP.Processors;

public class ManagementRequestProcessor : IRequestProcessor
{
    private const string ReadOperation = "READ";

    int IRequestProcessor.Credit => 100;

    public void Process(RequestContext requestContext)
    {
        var message = requestContext.Message;
        var operation = (string)message.ApplicationProperties.Map["operation"];
        if (operation != ReadOperation)
        {
            return;
        }

        var response = CreateResponseMessage(requestContext);
        requestContext.Complete(response);
    }

    private static Message CreateResponseMessage(RequestContext requestContext)
    {
        return new Message(
            new Map
            {
                { "partition_ids", new[] { "0" } },
                { "name", requestContext.ExtractTopicName() },
                { "created_at", DateTime.Now }
            }
        )
        {
            ApplicationProperties = new ApplicationProperties
            {
                ["status-code"] = 200
            }
        };
    }
}