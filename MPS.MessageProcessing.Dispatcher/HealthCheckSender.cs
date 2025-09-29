
using System.Net.Http.Json;
using MPS.MessageProcessing.Core.Models;

namespace MPS.MessageProcessing.Dispatcher;

//این کلاس HealthCheck را هر 30 ثانیه ارسال می‌کند
//در صورت خطا تا 5 بار Retry دارد
//اگر موفق نشد، سرویس غیرفعال می‌شود (IsEnabled = false)
public class HealthCheckSender
{
    private readonly HttpClient _httpClient;
    private readonly string _managementUrl;
    private readonly string _id;

    public HealthCheckSender(HttpClient httpClient, string managementUrl)
    {
        _httpClient = httpClient;
        _managementUrl = managementUrl;
        _id = Guid.NewGuid().ToString();
    }

    public async Task<HealthCheckResponse?> SendHealthCheckAsync(int connectedClients)
    {
        var request = new HealthCheckRequest
        {
            Id = _id,
            SystemTime = DateTime.UtcNow,
            NumberOfConnectedClients = connectedClients
        };

        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_managementUrl, request);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
            }
            catch
            {
                // Ignore, retry
            }
            await Task.Delay(10000);
        }

        // اگر موفق نشد، غیرفعال کردن سرویس
        return new HealthCheckResponse { IsEnabled = false, NumberOfActiveClients = 0, ExpirationTime = DateTime.UtcNow };
    }
}