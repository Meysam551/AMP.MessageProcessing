using System.Text.Json;
using System.Text.RegularExpressions;
using Grpc.Core;
using Grpc.Net.Client;
using MessageProcessing.Dispatcher.Grpc;
using MPS.Shared;
using GrpcModels = MessageProcessing.Dispatcher.Grpc;

namespace MPS.MessageProcessing.Processor;

public class ProcessorClient
{
    private readonly string _engineType;
    private readonly string _dispatcherUrl;
    private Dictionary<string, string> _regexSettings = new();

    public ProcessorClient(string engineType, string dispatcherUrl)
    {
        _engineType = engineType;
        _dispatcherUrl = dispatcherUrl;
    }

    public async Task RunAsync()
    {
        using var channel = GrpcChannel.ForAddress(_dispatcherUrl);
        var client = new GrpcModels.MessageProcessor.MessageProcessorClient(channel);

        using var call = client.Connect();

        // ارسال معرفی Processor
        await call.RequestStream.WriteAsync(new GrpcModels.ProcessorInfo
        {
            Id = Guid.NewGuid().ToString(),
            EngineType = _engineType
        });

        // دریافت پیام‌ها و پردازش
        await foreach (var message in call.ResponseStream.ReadAllAsync())
        {
            if (message.Sender == "DispatcherConfig")
            {
                var config = JsonSerializer.Deserialize<ProcessorConfig>(message.Content);
                _regexSettings = config!.RegexSettings;
                Console.WriteLine("Received regex configuration from Dispatcher");
                continue;
            }

            // پردازش پیام معمولی با Regex
            var additionalFields = new Dictionary<string, bool>();
            foreach (var kvp in _regexSettings)
                additionalFields[kvp.Key] = Regex.IsMatch(message.Content, kvp.Value);

            var processed = new ProcessedMessage
            {
                Id = message.Id,
                Engine = _engineType,
                MessageLength = message.Content.Length,
                IsValid = true
            };

            // اضافه کردن نتایج Regex
            foreach (var kvp in _regexSettings)
            {
                processed.AdditionalFields[kvp.Key] = Regex.IsMatch(message.Content, kvp.Value);
            }

            await client.SendProcessedMessageAsync(processed);
        }

        //await foreach (var message in call.ResponseStream.ReadAllAsync())
        //{
        //    Console.WriteLine($"Processing message {message.Id}");

        //    // مدل Core برای پردازش داخلی
        //    var processedCore = new CoreModels.ProcessedMessage
        //    {
        //        Id = message.Id,
        //        Engine = _engineType,
        //        MessageLength = message.Content.Length,
        //        IsValid = true
        //    };

        //    // فرض کن Dictionary از Dispatcher دریافت می‌کنیم
        //    var regexSettings = message.RegexSettings; // map<string,string> در gRPC

        //    // اجرای Regexها
        //    foreach (var kvp in regexSettings)
        //    {
        //        try
        //        {
        //            var isMatch = Regex.IsMatch(message.Content, kvp.Value);
        //            processedCore.RegexResults.Add(kvp.Key, isMatch);
        //        }
        //        catch
        //        {
        //            processedCore.RegexResults.Add(kvp.Key, false);
        //        }
        //    }

        //    // تبدیل به gRPC مدل برای ارسال
        //    var processedGrpc = new GrpcModels.ProcessedMessage
        //    {
        //        Id = processedCore.Id,
        //        Engine = processedCore.Engine,
        //        MessageLength = processedCore.MessageLength,
        //        IsValid = processedCore.IsValid
        //    };

        //    foreach (var kvp in processedCore.RegexResults)
        //    {
        //        processedGrpc.RegexResults.Add(kvp.Key, kvp.Value);
        //    }

        //    await client.SendProcessedMessageAsync(processedGrpc);
        //}
    }
}
