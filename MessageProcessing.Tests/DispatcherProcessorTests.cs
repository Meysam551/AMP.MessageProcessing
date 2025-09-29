
using Grpc.Core;
using DispatcherProto = MPS.MessageProcessing.Dispatcher;
using DispatcherGrpcProto = MPS.MessageProcessing.Dispatcher.GrpcServer;
using ProcessorProto = MPS.MessageProcessing.Processor;
using MPS.MessageProcessingProto.Dispatcher.Grpc;

namespace MessageProcessing.Tests
{
    public class DispatcherProcessorTests
    {
        [Fact]
        public async Task Processor_Should_ProcessMessages_And_SendResultsToDispatcher()
        {
            // شبیه‌سازی Queue
            var queue = new DispatcherProto.MessageQueueSimulator();

            // ایجاد Dispatcher Service
            var dispatcher = new DispatcherGrpcProto.MessageProcessorService(queue);

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
                // تنظیم Dynamic Regex برای تست
                var regexSettings = new Dictionary<string, string>
            {
                { "ContainsNumber", @"\d+" },
                { "ContainsHello", @"\bhello\b" }
            };

                // ایجاد Processor Client
                var processor = new ProcessorProto.ProcessorClient("RegexEngineTest", $"http://localhost:{Port}", regexSettings);

                // اجرای Processor به صورت غیرهمزمان
                var processorTask = processor.RunAsync();

                // اجازه بده چند پیام پردازش شوند
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
