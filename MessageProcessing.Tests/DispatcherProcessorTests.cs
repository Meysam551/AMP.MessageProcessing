using Grpc.Core;
using MPS.MessageProcessing.Dispatcher;
using MPS.MessageProcessing.Dispatcher.GrpcServer;
using MPS.MessageProcessing.Processor;
using MPS.MessageProcessingProto.Dispatcher.Grpc;

namespace MessageProcessing.Tests
{
    public class DispatcherProcessorTests
    {
        [Fact]
        public async Task Processor_Should_ProcessMessages_And_SendResultsToDispatcher()
        {
            // Arrange
            var queue = new MessageQueueSimulator();
            var dispatcher = new MessageProcessorService(queue);

            const int Port = 5002;
            var server = new Server
            {
                Services = { MessageProcessor.BindService(dispatcher) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            server.Start();
            var dispatcherAddress = $"http://localhost:{Port}";
            var processor = new ProcessorClient("RegexEngineTest", dispatcherAddress);

            // Cancellation برای کنترل خروج از حلقه
            using var cts = new CancellationTokenSource();

            // Act
            var processorTask = processor.RunAsync(cts.Token);
            await Task.Delay(3000);

            // Assert
            var results = dispatcher.GetProcessedResults().ToList();
            Assert.NotEmpty(results);

            Assert.All(results, m =>
            {
                Assert.True(m.MessageLength > 0);
                Assert.Equal("RegexEngineTest", m.Engine);
            });

            // Cleanup: توقف ایمن
            cts.Cancel();
            await Task.WhenAny(processorTask, Task.Delay(1000)); 

            await server.ShutdownAsync();
        }
    }
}
