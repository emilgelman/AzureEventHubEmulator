using System.Diagnostics;

namespace AzureEventHubEmulator.IntegrationTests;

public class IntegrationTest : IClassFixture<IntegrationTestFixture>
{
    private readonly Consumer? _consumer;
    private readonly Producer? _producer;

    public IntegrationTest(IntegrationTestFixture fixture)
    {
        _consumer = fixture._consumer;
        _producer = fixture._producer;
    }

    [Fact]
    public async Task TestProduceConsume()
    {
        await _producer.SendAsync("hello world");
        await _consumer.StartAsync();
        await AssertEventuallyAsync(() => _consumer.GetEvents().Any(), TimeSpan.FromSeconds(60));
        Assert.Equal("hello world", _consumer.GetEvents().First());
    }

    private static async Task AssertEventuallyAsync(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(1000);
        }

        Assert.True(condition());
    }
}