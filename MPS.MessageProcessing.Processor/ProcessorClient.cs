
using System.Text.Json;
using System.Text.RegularExpressions;
using Grpc.Core;
using Grpc.Net.Client;
using MPS.MessageProcessingProto.Dispatcher.Grpc;
using GrpcModels = MPS.MessageProcessingProto.Dispatcher.Grpc; // مطمئن شو این alias با پروژه‌ات یکیه
// اگر توی فایلهات alias متفاوته، همون رو بذار

namespace MPS.MessageProcessing.Processor;

/// <summary>
/// ProcessorClient: gRPC client که به Dispatcher وصل می‌شود،
/// کانفیگ regex را از Dispatcher دریافت می‌کند یا می‌توان آن را از طریق constructor تزریق کرد،
/// پیام‌ها را پردازش می‌کند و نتایج را باز می‌فرستد.
/// </summary>
public class ProcessorClient
{
    private readonly string _engineType;
    private readonly string _dispatcherUrl;
    private Dictionary<string, string> _regexSettings = new();

    // سازنده اصلی (بدون regexSettings) — برای حالت واقعی که کانفیگ از Dispatcher می‌آید
    public ProcessorClient(string engineType, string dispatcherUrl)
    {
        _engineType = engineType ?? throw new ArgumentNullException(nameof(engineType));
        _dispatcherUrl = dispatcherUrl ?? throw new ArgumentNullException(nameof(dispatcherUrl));
    }

    // overload برای تست یا حالت تزریق دستی تنظیمات
    public ProcessorClient(string engineType, string dispatcherUrl, Dictionary<string, string> regexSettings)
        : this(engineType, dispatcherUrl)
    {
        _regexSettings = regexSettings ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// پردازش یک پیام دریافتی (از نوع MessageToProcess).
    /// اگر پیام از نوع کانفیگ (Sender == "DispatcherConfig") باشد،
    /// تنظیمات regex درون‌شه‌ای آپدیت می‌شود و مقدار null برمی‌گردد.
    /// در غیر این صورت یک Grpc ProcessedMessage ساخته و بازگردانده می‌شود.
    /// </summary>
    public GrpcModels.ProtoProcessedMessage? HandleMessage(GrpcModels.ProtoMessageToProcess message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        // پیام کانفیگ — محتوی JSON یک ProcessorConfig است
        if (string.Equals(message.Sender, "DispatcherConfig", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var config = JsonSerializer.Deserialize<ProcessorConfig>(message.Content ?? "{}");
                if (config?.RegexSettings != null)
                {
                    _regexSettings = new Dictionary<string, string>(config.RegexSettings);
                }
            }
            catch
            {
                // اگر دسیریالایز خطا داد، تنظیمات تغییر نکند (و بهتره لاگ بشه)
            }
            return null; // پیام کانفیگ خروجی پردازشی ندارد
        }

        // پیام عادی => پردازش
        var processed = new GrpcModels.ProtoProcessedMessage
        {
            Id = message.Id,
            Engine = _engineType,
            MessageLength = message.Content?.Length ?? 0,
            IsValid = true
        };

        return processed;
    }

    /// <summary>
    /// متد اصلی اجرا که به Dispatcher وصل می‌شود، معرفی می‌کند،
    /// پیام‌ها را دریافت می‌کند و با استفاده از HandleMessage نتایج را ارسال می‌کند.
    /// </summary>
    public async Task RunAsync()
    {
        using var channel = GrpcChannel.ForAddress(_dispatcherUrl);
        var client = new MessageProcessor.MessageProcessorClient(channel);

        using var call = client.Connect();

        // ارسال معرفی Processor
        await call.RequestStream.WriteAsync(new GrpcModels.ProcessorInfo
        {
            Id = Guid.NewGuid().ToString(),
            EngineType = _engineType
        });

        // حلقه دریافت پیام‌ها
        await foreach (var message in call.ResponseStream.ReadAllAsync())
        {
            var processed = HandleMessage(message);
            if (processed != null)
            {
                await client.SendProcessedMessageAsync(processed);
            }
        }
    }
}

public class ProcessorConfig
{
    public Dictionary<string, string> RegexSettings { get; set; } = new();
}
