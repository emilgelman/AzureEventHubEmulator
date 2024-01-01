using System.Threading.Channels;
using Amqp;

namespace AzureEventHubEmulator.Entities;

public class Topic
{
    private readonly Channel<Message> _eventChannel;

    public Topic()
    {
        _eventChannel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }

    public ChannelReader<Message> Reader() => _eventChannel.Reader;


    public void PublishMessage(Message message)
    {
        if (!_eventChannel.Writer.TryWrite(message))
        {
            throw new Exception("Could not write to channel");
        }
    }
}