using System.Net.Http;
using System.Net.Http.Json;

namespace MessageProcessing.Dispatcher.Services;

public class HealthCheckService
{
    private readonly HttpClient _client;
    private readonly string _healthUrl;

    public HealthCheckService(string healthUrl)
    {
        _healthUrl = healthUrl;
        _client = new HttpClient();
    }

    public async Task<HealthResponse?> CheckHealthAsync(int numberOfConnectedClients)
    {
        var request = new HealthRequest
        {
            Id = Guid.NewGuid().ToString(),
            SystemTime = DateTime.UtcNow,
            NumberOfConnectedClients = numberOfConnectedClients
        };

        try
        {
            var response = await _client.PostAsJsonAsync(_healthUrl, request);
            if (!response.IsSuccessStatusCode) return null;

            var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
            return health;
        }
        catch
        {
            // اگر مشکلی در ارتباط بود null برگردان
            return null;
        }
    }
}

public record HealthRequest
{
    public string Id { get; init; } = string.Empty;
    public DateTime SystemTime { get; init; }
    public int NumberOfConnectedClients { get; init; }
}

public record HealthResponse
{
    public bool IsEnabled { get; init; }
    public int NumberOfActiveClients { get; init; }
    public DateTime ExpirationTime { get; init; }
}
