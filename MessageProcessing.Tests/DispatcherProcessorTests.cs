
using Grpc.Core;
using MPS.MessageProcessing.Dispatcher;
using MPS.MessageProcessing.Dispatcher.GrpcServer;
using MPS.MessageProcessing.Dispatcher.Models;
using MPS.MessageProcessing.Processor;
using MPS.MessageProcessingProto.Dispatcher.Grpc;
using DispatcherGrpcProto = MPS.MessageProcessing.Dispatcher.GrpcServer;
using DispatcherProto = MPS.MessageProcessing.Dispatcher;
using ProcessorProto = MPS.MessageProcessing.Processor;

namespace MessageProcessing.Tests
{
    /// <summary>
    /// مجموعه تست‌ها برای بررسی عملکرد Dispatcher و Processor
    /// </summary>
    public class DispatcherProcessorTests
    {
        [Fact]
        public async Task Processor_Should_ProcessMessages_And_SendResultsToDispatcher()
        {
            // شبیه‌سازی Queue
            var queue = new MessageQueueSimulator();

            // ایجاد Dispatcher Service
            var dispatcher = new MessageProcessorService(queue);

            // اجرای gRPC Server در پورت تصادفی
            const int Port = 5002;
            var server = new Server
            {
                Services = { MessageProcessor.BindService(dispatcher) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            try
            {
                var processor = new ProcessorClient("RegexEngineTest", $"http://localhost:{Port}");

                var processorTask = processor.RunAsync();

                await Task.Delay(2000);

                // بررسی پیام‌های پردازش شده
                var results = dispatcher.GetProcessedResults().ToList();
                Assert.NotEmpty(results);
                Assert.All(results, m =>
                {
                    Assert.True(m.MessageLength > 0);
                    Assert.Equal("RegexEngineTest", m.Engine);
                });

                // متوقف کردن Processor
                processorTask.Dispose();
            }
            finally
            {
                await server.ShutdownAsync();
            }
        }
    }
}
