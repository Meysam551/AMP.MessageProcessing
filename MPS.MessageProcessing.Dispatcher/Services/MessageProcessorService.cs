using System.Collections.Concurrent;
using System.Text.Json;
using Grpc.Core;
using MessageProcessing.Dispatcher.Grpc;
using MessageProcessing.Dispatcher.Services;
using MPS.MessageProcessing.Dispatcher.Models;
using MPS.Shared;


namespace MPS.MessageProcessing.Dispatcher.GrpcServer;

public class MessageProcessorService : MessageProcessor.MessageProcessorBase
{
    private readonly MessageQueueSimulator _queue;
    //private readonly ConcurrentDictionary<string, IServerStreamWriter<MessageToProcess>> _connectedProcessors = new();
    private readonly ConcurrentDictionary<string, ProcessorState> _connectedProcessors = new();

    // حافظه برای ذخیره نتایج پردازش
    private readonly ConcurrentBag<ProcessedMessage> _processedResults = new();


    public MessageProcessorService(MessageQueueSimulator queue)
    {
        _queue = queue;
    }

    public override async Task Connect(IAsyncStreamReader<ProcessorInfo> requestStream,
                                       IServerStreamWriter<MessageToProcess> responseStream,
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

            // ارسال تنظیمات Regex به Processor
            var config = new ProcessorConfig
            {
                RegexSettings = new Dictionary<string, string>
            {
                { "ContainsNumber", @"\d+" },
                { "ContainsHello", @"\bhello\b" }
            }
            };

            // اینجا باید روشی داشته باشیم که config را به Processor ارسال کند
            await responseStream.WriteAsync(new MessageToProcess
            {
                Id = 0,
                Content = JsonSerializer.Serialize(config),
                Sender = "DispatcherConfig"
            });

            // ادامه ارسال پیام‌ها
            _ = Task.Run(async () =>
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var message = await _queue.GetNextMessageAsync();
                    var msgToSend = new MessageToProcess
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


    public override Task<Ack> SendProcessedMessage(ProcessedMessage request, ServerCallContext context)
    {
        Console.WriteLine($"Received processed message {request.Id} from {request.Engine}");

        // ذخیره در صف نتایج (در این مثال ConcurrentBag برای شبیه‌سازی)
        _processedResults.Add(request);

        // اگر بخوای می‌توانی همزمان log یا Database هم استفاده کنی
        return Task.FromResult(new Ack { Success = true });
    }

    public IEnumerable<ProcessedMessage> GetProcessedResults()
    {
        return _processedResults.ToArray();
    }

    public async Task MonitorProcessorsAsync(HealthCheckService healthService)
    {
        while (true)
        {
            var activeClients = _connectedProcessors.Values.Count(p => p.IsActive);
            var health = await healthService.CheckHealthAsync(activeClients);

            if (health == null || !health.IsEnabled)
            {
                // همه Processorها را غیرفعال کن
                foreach (var p in _connectedProcessors.Values)
                    p.IsActive = false;
            }
            else
            {
                // تعداد Processorهای فعال را با مقدار دریافتی از HealthCheck هماهنگ کن
                var toActivate = health.NumberOfActiveClients - activeClients;
                if (toActivate > 0)
                {
                    foreach (var p in _connectedProcessors.Values.Where(p => !p.IsActive).Take(toActivate))
                        p.IsActive = true;
                }

                // غیرفعال کردن Processorهایی که بیش از 5 دقیقه پیام نخوانده‌اند
                var timeout = DateTime.UtcNow.AddMinutes(-5);
                foreach (var p in _connectedProcessors.Values.Where(p => p.LastRequestTime < timeout))
                    p.IsActive = false;
            }

            await Task.Delay(30000); // هر 30 ثانیه بررسی کن
        }
    }


}
