using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Grpc.Core;
using MPS.MessageProcessing.Dispatcher.Models;
using MPS.MessageProcessingProto.Dispatcher.Grpc;
using MPS.Shared;

namespace MPS.MessageProcessing.Dispatcher.GrpcServer;

/// <summary>
/// gRPC service responsible for coordinating Processors and managing message distribution.
/// </summary>
public class MessageProcessorService : MessageProcessor.MessageProcessorBase
{
    private readonly MessageQueueSimulator _queue;
    private readonly ConcurrentDictionary<string, ProcessorState> _connectedProcessors = new();
    private readonly ConcurrentBag<ProtoProcessedMessage> _processedResults = new();
    private readonly HttpClient _httpClient = new();
    private readonly string _managementUrl;
    private bool _isEnabled = true;
    private DateTime _expirationTime = DateTime.UtcNow.AddMinutes(10);
    private int _activeClients = 5;

    public MessageProcessorService(MessageQueueSimulator queue, string managementUrl = "http://localhost:5000/api/module/health")
    {
        _queue = queue;
        _managementUrl = managementUrl;

        // Start periodic health monitoring
        _ = Task.Run(HealthCheckLoopAsync);
    }

    /// <summary>
    /// Bi-directional gRPC connection with Processors
    /// </summary>
    public override async Task Connect(
        IAsyncStreamReader<ProcessorInfo> requestStream,
        IServerStreamWriter<ProtoMessageToProcess> responseStream,
        ServerCallContext context)
    {
        await foreach (var processor in requestStream.ReadAllAsync(context.CancellationToken))
        {
            // Register processor connection
            _connectedProcessors.TryAdd(processor.Id, new ProcessorState
            {
                Stream = responseStream,
                IsActive = true,
                LastRequestTime = DateTime.UtcNow
            });

            Console.WriteLine($"✅ Processor connected: {processor.Id} ({processor.EngineType})");

            // Send initial config
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
                Sender = "DispatcherConfig",
                Content = JsonSerializer.Serialize(config)
            });

            // Start message streaming if active
            _ = Task.Run(async () =>
            {
                while (!context.CancellationToken.IsCancellationRequested && _isEnabled)
                {
                    var msg = await _queue.GetNextMessageAsync();
                    var msgToSend = new ProtoMessageToProcess
                    {
                        Id = msg.Id,
                        Sender = msg.Sender,
                        Content = msg.Content
                    };

                    await responseStream.WriteAsync(msgToSend);
                    await Task.Delay(200, context.CancellationToken);
                }
            }, context.CancellationToken);
        }
    }

    /// <summary>
    /// Receives processed messages from Processors and stores them.
    /// </summary>
    public override Task<Ack> SendProcessedMessage(ProtoProcessedMessage request, ServerCallContext context)
    {
        Console.WriteLine($"Received processed message {request.Id} from {request.Engine}");

        _processedResults.Add(request);

        return Task.FromResult(new Ack { Success = true });
    }

    /// <summary>
    /// Exposed for unit tests to verify processed messages.
    /// </summary>
    public IEnumerable<ProtoProcessedMessage> GetProcessedResults() => _processedResults;

    /// <summary>
    /// Periodically sends health check requests to the Management API.
    /// </summary>
    private async Task HealthCheckLoopAsync()
    {
        while (true)
        {
            try
            {
                var req = new HealthRequestModel
                {
                    Id = Guid.NewGuid().ToString(),
                    SystemTime = DateTime.UtcNow,
                    NumberOfConnectedClients = _connectedProcessors.Count
                };

                var response = await _httpClient.PostAsJsonAsync(_managementUrl, req);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<HealthResponseModel>();
                    if (data != null)
                    {
                        _isEnabled = data.IsEnabled;
                        _activeClients = data.NumberOfActiveClients;
                        _expirationTime = data.ExpirationTime;

                        Console.WriteLine($"HealthCheck OK — ActiveClients={_activeClients}, Expire={_expirationTime:T}");
                    }
                }
                else
                {
                    Console.WriteLine($"HealthCheck failed with {response.StatusCode}");
                    await HandleHealthFailureAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HealthCheck error: {ex.Message}");
                await HandleHealthFailureAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }

    private async Task HandleHealthFailureAsync()
    {
        // Retry 5 times before disabling
        for (int i = 0; i < 5; i++)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                var req = new HealthRequestModel
                {
                    Id = Guid.NewGuid().ToString(),
                    SystemTime = DateTime.UtcNow,
                    NumberOfConnectedClients = _connectedProcessors.Count
                };

                var response = await _httpClient.PostAsJsonAsync(_managementUrl, req);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("HealthCheck recovered!");
                    return;
                }
            }
            catch
            {
                // ignore
            }
        }

        Console.WriteLine("HealthCheck failed after retries — disabling message distribution.");
        _isEnabled = false;
    }
}

public class ProcessorConfig
{
    public Dictionary<string, string> RegexSettings { get; set; } = new();
}
