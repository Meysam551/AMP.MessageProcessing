
using MPS.MessageProcessing.Dispatcher;

namespace MessageProcessing.Dispatcher.Tests;

public class DispatcherTests
{
    [Fact]
    public async Task Queue_Should_ReturnMessage()
    {
        var queue = new MessageQueueSimulator();
        var msg = await queue.GetNextMessageAsync();
        Assert.NotNull(msg);
        Assert.True(msg.Id > 0);
        Assert.NotEmpty(msg.Content);
    }
}