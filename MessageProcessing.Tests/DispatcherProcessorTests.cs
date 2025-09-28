
using Grpc.Core;
using DispacherGrc = MessageProcessing.Dispatcher.Grpc;
using MPS.MessageProcessing.Dispatcher;
using MPS.MessageProcessing.Dispatcher.GrpcServer;

namespace MessageProcessing.Tests
{
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
                Services = { DispacherGrc.MessageProcessor.BindService(dispatcher) },
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
                var processor = new ProcessorClient("RegexEngineTest", $"http://localhost:{Port}", regexSettings);

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
                    // بررسی Dynamic Regex
                    Assert.Contains("ContainsNumber", m.AdditionalFields.Keys);
                    Assert.Contains("ContainsHello", m.AdditionalFields.Keys);
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
