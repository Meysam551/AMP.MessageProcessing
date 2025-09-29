
using System.Net.Http.Json;

namespace MessageProcessing.Dispatcher.Services;

/// <summary>
/// سرویس بررسی سلامت سامانه مدیریت.
/// این سرویس هر بار که فراخوانی شود، وضعیت سیستم را از API HealthCheck دریافت می‌کند.
/// </summary>
public class HealthCheckService
{
    private readonly HttpClient _client;
    private readonly string _healthUrl;

    /// <summary>
    /// سازنده سرویس
    /// </summary>
    /// <param name="healthUrl">آدرس API HealthCheck سامانه مدیریت</param>
    public HealthCheckService(string healthUrl)
    {
        _healthUrl = healthUrl;
        _client = new HttpClient();
    }

    /// <summary>
    /// بررسی وضعیت سلامت سامانه مدیریت
    /// </summary>
    /// <param name="numberOfConnectedClients">تعداد پردازش‌کننده‌های متصل شده</param>
    /// <returns>
    /// شیء <see cref="HealthResponse"/> در صورت موفقیت،
    /// یا null در صورت بروز خطا یا پاسخ غیر موفق از سرور.
    /// </returns>
    public async Task<HealthResponse?> CheckHealthAsync(int numberOfConnectedClients)
    {
        var request = new HealthRequest
        {
            Id = Guid.NewGuid().ToString(),         // شناسه یکتا برای درخواست
            SystemTime = DateTime.UtcNow,           // زمان سیستم
            NumberOfConnectedClients = numberOfConnectedClients
        };

        try
        {
            // ارسال درخواست POST به API HealthCheck
            var response = await _client.PostAsJsonAsync(_healthUrl, request);
            if (!response.IsSuccessStatusCode)
                return null;

            // خواندن پاسخ به صورت JSON و تبدیل به HealthResponse
            var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
            return health;
        }
        catch
        {
            // اگر مشکلی در ارتباط بود (مانند خطای شبکه) null برگردان
            return null;
        }
    }
}

/// <summary>
/// مدل درخواست HealthCheck
/// </summary>
public record HealthRequest
{
    public string Id { get; init; } = string.Empty;        // شناسه یکتا برای هر درخواست
    public DateTime SystemTime { get; init; }             // زمان سیستم هنگام ارسال درخواست
    public int NumberOfConnectedClients { get; init; }    // تعداد پردازش‌کننده‌های متصل
}

/// <summary>
/// مدل پاسخ HealthCheck
/// </summary>
public record HealthResponse
{
    public bool IsEnabled { get; init; }                 // وضعیت فعال بودن سامانه تقسیم پیام
    public int NumberOfActiveClients { get; init; }      // تعداد پردازش‌کننده‌های فعال
    public DateTime ExpirationTime { get; init; }        // زمان انقضای اعتبار پاسخ
}

