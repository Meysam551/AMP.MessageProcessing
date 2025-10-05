using System.Collections.Concurrent;
using System.Text.Json;
using Grpc.Core;
using MPS.MessageProcessingProto.Dispatcher.Grpc;
using MPS.MessageProcessing.Dispatcher.Models;

namespace MPS.MessageProcessing.Dispatcher.GrpcServer
{
    /// <summary>
    /// سرویس gRPC برای مدیریت ارتباط با پردازشگرها (Processors).
    /// این کلاس مسئول استقرار پردازشگرها، ارسال پیام‌ها از صف شبیه‌ساز و دریافت نتایج پردازش است.
    /// </summary>
    public class MessageProcessorService : MessageProcessor.MessageProcessorBase
    {
        private readonly MessageQueueSimulator _queue;
        private readonly ConcurrentDictionary<string, ProcessorState> _connectedProcessors = new();

        // ✅ اضافه‌شده: محل ذخیره نتایج برای تست
        private readonly ConcurrentBag<ProtoProcessedMessage> _processedResults = new();

        public MessageProcessorService(MessageQueueSimulator queue)
        {
            _queue = queue;
        }

        public override async Task Connect(
            IAsyncStreamReader<ProcessorInfo> requestStream,
            IServerStreamWriter<ProtoMessageToProcess> responseStream,
            ServerCallContext context)
        {
            await foreach (var processor in requestStream.ReadAllAsync())
            {
                _connectedProcessors.TryAdd(processor.Id, new ProcessorState
                {
                    Stream = responseStream,
                    IsActive = true,
                    LastRequestTime = DateTime.UtcNow
                });

                Console.WriteLine($"✅ Processor متصل شد: {processor.Id} ({processor.EngineType})");

                var config = new ProcessorConfig
                {
                    RegexSettings = new Dictionary<string, string>
                    {
                        { "ContainsNumber", @"\d+" },
                        { "ContainsHello", @"\bhello\b" }
                    }
                };

                await responseStream.WriteAsync(new ProtoMessageToProcess
                {
                    Id = 0,
                    Content = JsonSerializer.Serialize(config),
                    Sender = "DispatcherConfig"
                });

                _ = Task.Run(async () =>
                {
                    while (!context.CancellationToken.IsCancellationRequested)
                    {
                        var message = await _queue.GetNextMessageAsync();

                        var msgToSend = new ProtoMessageToProcess
                        {
                            Id = message.Id,
                            Sender = message.Sender,
                            Content = message.Content
                        };

                        await responseStream.WriteAsync(msgToSend);
                        await Task.Delay(200);
                    }
                });
            }
        }

        public override Task<Ack> SendProcessedMessage(
            ProtoProcessedMessage request,
            ServerCallContext context)
        {
            Console.WriteLine($"📩 دریافت پیام پردازش ‌شده {request.Id} از {request.Engine}");

            // ✅ اضافه‌شده برای تست
            _processedResults.Add(request);

            // اینجا میشه هم تو دیتابیس ذخیره کرد هم تو EventStore
            return Task.FromResult(new Ack { Success = true });
        }

        /// <summary>
        /// متد کمکی برای Unit Test جهت بررسی نتایج پردازش.
        /// </summary>
        public IEnumerable<ProtoProcessedMessage> GetProcessedResults()
        {
            // ✅ الان واقعاً داده داره
            return _processedResults.ToArray();
        }
    }

    public class ProcessorConfig
    {
        public Dictionary<string, string> RegexSettings { get; set; } = new();
    }
}


