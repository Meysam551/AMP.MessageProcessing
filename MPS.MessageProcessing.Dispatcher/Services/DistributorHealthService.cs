using System.Net.Http.Json;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPS.Shared;

namespace MPS.MessageProcessing.Dispatcher.Services;

/// <summary>
/// وظیفه دارد هر 30 ثانیه وضعیت Health سامانه تقسیم پیام را به سامانه مدیریت بفرستد.
/// در صورت بروز خطا، فقط هشدار لاگ می‌کند (سرویس را قطع نمی‌کند).
/// </summary>
using System.Net.NetworkInformation;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPS.Shared;

public class DistributorHealthService : BackgroundService
{
    private readonly HttpClient _http;
    private readonly ILogger<DistributorHealthService> _logger;
    private readonly IConfiguration _config;
    private readonly Func<int> _getConnectedClients;

    public DistributorHealthService(
        HttpClient http,
        ILogger<DistributorHealthService> logger,
        IConfiguration config)
    {
        _http = http;
        _logger = logger;
        _config = config;

        // چون فعلاً تعداد کلاینت‌ها ثابت فرض می‌کنیم
        _getConnectedClients = () => 5;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var managementUrl = _config["Management:HealthUrl"]
            ?? "https://localhost:5001/api/module/health";

        _logger.LogInformation("DistributorHealthService started. Target: {url}", managementUrl);

        //تا زمانیکه سرویس در حالن توقف نباشه ادامه میده به چک کردن سلامت سیستم
        // و تعداد پردازشگرهای فعال از طریق آدرس بالا
        while (!stoppingToken.IsCancellationRequested)
        {
            // درخواست ایجاد میشود
            var request = new HealthRequestModel
            {
                Id = GenerateSystemGuid(),
                SystemTime = DateTime.UtcNow,
                NumberOfConnectedClients = _getConnectedClients()
            };

            try
            {
                // درخواست بصورت Json ارسال میشه
                var response = await _http.PostAsJsonAsync(managementUrl, request, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    var health = await response.Content.ReadFromJsonAsync<HealthResponseModel>(cancellationToken: stoppingToken);
                    _logger.LogInformation(" HealthCheck OK: Enabled={enabled}, ActiveClients={count}",
                        health?.IsEnabled, health?.NumberOfActiveClients);
                }
                else
                {
                    _logger.LogWarning(" HealthCheck failed: {status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error sending HealthCheck to ManagementServer");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private static string GenerateSystemGuid()
    {
        try
        {
            var mac = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Select(n => n.GetPhysicalAddress()?.GetAddressBytes())
                .FirstOrDefault(b => b?.Length > 0);

            if (mac == null)
                return Guid.NewGuid().ToString();

            var padded = new byte[16];
            Array.Copy(mac, 0, padded, 0, Math.Min(mac.Length, 6));
            return new Guid(padded).ToString();
        }
        catch
        {
            return Guid.NewGuid().ToString();
        }
    }
}
