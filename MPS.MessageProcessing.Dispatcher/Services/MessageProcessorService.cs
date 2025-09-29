using System.Collections.Concurrent;
using System.Text.Json;
using Grpc.Core;
using MPS.MessageProcessingProto.Dispatcher.Grpc;
using MPS.MessageProcessing.Dispatcher.Models;

namespace MPS.MessageProcessing.Dispatcher.GrpcServer;

/// <summary>
/// سرویس gRPC برای مدیریت ارتباط با پردازشگرها (Processors).
/// این کلاس مسئول استقرار پردازشگرها، ارسال پیام‌ها از صف شبیه‌ساز و دریافت نتایج پردازش است.
/// </summary>
public class MessageProcessorService : MessageProcessor.MessageProcessorBase
{
    private readonly MessageQueueSimulator _queue;
    private readonly ConcurrentDictionary<string, ProcessorState> _connectedProcessors = new();

    /// <summary>
    /// ایجاد سرویس MessageProcessorService
    /// </summary>
    /// <param name="queue">صف پیام شبیه‌سازی‌شده که قرار است پیام‌ها از آن خوانده شوند.</param>
    public MessageProcessorService(MessageQueueSimulator queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// متد اصلی برای اتصال پردازشگرها به Dispatcher.
    /// - اضافه کردن پردازشگر به لیست پردازشگرهای متصل
    /// - ارسال تنظیمات اولیه (Regex config)
    /// - ارسال پیام‌ها به پردازشگر به صورت Stream
    /// </summary>
    public override async Task Connect(
        IAsyncStreamReader<ProcessorInfo> requestStream,
        IServerStreamWriter<ProtoMessageToProcess> responseStream,
        ServerCallContext context)
    {
        await foreach (var processor in requestStream.ReadAllAsync())
        {
            // ذخیره وضعیت پردازشگر متصل
            _connectedProcessors.TryAdd(processor.Id, new ProcessorState
            {
                Stream = responseStream,
                IsActive = true,
                LastRequestTime = DateTime.UtcNow
            });

            Console.WriteLine($"✅ Processor متصل شد: {processor.Id} ({processor.EngineType})");

            // ارسال تنظیمات Regex به پردازشگر
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

            // ارسال پیام‌های صف به این Processor
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
                    await Task.Delay(200); // جلوگیری از overload
                }
            });
        }
    }

    /// <summary>
    /// دریافت پیام پردازش‌شده از Processor.
    /// در اینجا می‌توان نتایج را لاگ یا در دیتابیس ذخیره کرد.
    /// </summary>
    public override Task<Ack> SendProcessedMessage(
        ProtoProcessedMessage request,
        ServerCallContext context)
    {
        Console.WriteLine($" دریافت پیام پردازش ‌شده {request.Id} از {request.Engine}");

        // اینجا رو میشه هم تو دیتابیس ذخیره کرد هم تو EventStore
        return Task.FromResult(new Ack { Success = true });
    }

    /// <summary>
    /// متد کمکی برای Unit Test جهت بررسی نتایج پردازش.
    /// (در حال حاضر خالی برمی‌گردد.)
    /// </summary>
    public IEnumerable<ProtoProcessedMessage> GetProcessedResults()
    {
        // فرض می‌کنیم اینجا نتایج را جمع می‌کنیم
        return Enumerable.Empty<ProtoProcessedMessage>();
    }
}

/// <summary>
/// تنظیمات پیکربندی پردازشگر (مانند Regexها).
/// </summary>
public class ProcessorConfig
{
    public Dictionary<string, string> RegexSettings { get; set; } = new();
}

