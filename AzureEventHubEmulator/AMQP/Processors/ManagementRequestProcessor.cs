using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Amqp.Types;

namespace AzureEventHubEmulator.AMQP.Processors;

public class ManagementRequestProcessor : IRequestProcessor
{
    int IRequestProcessor.Credit => 100;

    void IRequestProcessor.Process(RequestContext requestContext)
    {
        Console.WriteLine("Received a request " + requestContext.Message.Body);
        var task = this.ReplyAsync(requestContext);
    }

    Task ReplyAsync(RequestContext requestContext)
    {
        var message = requestContext.Message;
        if ((string)message.ApplicationProperties.Map["operation"] == "READ")
        {
            var AmqpMap = new Map();
            AmqpMap.Add("partition_ids", new[] { "0" });
            AmqpMap.Add("name", "test");
            AmqpMap.Add("created_at", DateTime.Now);
            var response = new Message(AmqpMap);
            response.ApplicationProperties = new ApplicationProperties();
            response.ApplicationProperties["status-code"] = 200;
            requestContext.Complete(response);
        }

        return Task.CompletedTask;
    }
}