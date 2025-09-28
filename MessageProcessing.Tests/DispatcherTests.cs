using MessageProcessing.Dispatcher.Grpc;
using MPS.MessageProcessing.Dispatcher;
using MPS.MessageProcessing.Dispatcher.GrpcServer;

namespace MessageProcessing.Tests
{
    public class DispatcherTests
    {
        [Fact]
        public async Task SendProcessedMessage_ShouldStoreMessage()
        {
            var queue = new MessageQueueSimulator();
            var dispatcher = new MessageProcessorService(queue);

            var processedMessage = new ProcessedMessage
            {
                Id = 1,
                Engine = "RegexEngine",
                MessageLength = 12,
                IsValid = true
            };

            await dispatcher.SendProcessedMessage(processedMessage, null!);

            var results = dispatcher.GetProcessedResults();
            Assert.Contains(results, m => m.Id == 1 && m.Engine == "RegexEngine");
        }
    }
}
