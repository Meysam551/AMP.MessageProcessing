using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPS.MessageProcessing.Dispatcher;
using MPS.MessageProcessing.Dispatcher.GrpcServer;
using MPS.MessageProcessing.Dispatcher.Services;
using MPS.MessageProcessingProto.Dispatcher.Grpc;
using MPS.Shared;

Console.Title = "🧩 Message Dispatcher Service";

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // خواندن appsettings.json و متغیرهای محیطی
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        // وابستگی‌ها
        services.AddSingleton<MessageQueueSimulator>();
        services.AddSingleton<MessageProcessorService>();
        services.AddHttpClient<DistributorHealthService>();
        services.AddHostedService<DistributorHealthService>();
    });

var host = builder.Build();

// Load configuration
var config = host.Services.GetRequiredService<IConfiguration>();
var queue = host.Services.GetRequiredService<MessageQueueSimulator>();
var dispatcher = host.Services.GetRequiredService<MessageProcessorService>();

// گرفتن پورت از appsettings.json
int grpcPort = config.GetValue("Dispatcher:GrpcPort", 7001);

// اجرای gRPC Server
var server = new Server
{
    Services = { MessageProcessor.BindService(dispatcher) },
    Ports = { new ServerPort("localhost", grpcPort, ServerCredentials.Insecure) }
};

server.Start();

Console.WriteLine($"🚀 Message Dispatcher started on port {grpcPort}");
Console.WriteLine($"📡 HealthCheck target: {config["Management:HealthUrl"]}");
Console.WriteLine("Press any key to stop...");

await host.StartAsync();
Console.ReadKey();

Console.WriteLine("🛑 Stopping Dispatcher...");
await server.ShutdownAsync();
await host.StopAsync();

Console.WriteLine("✅ Dispatcher stopped successfully.");