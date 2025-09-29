
namespace MPS.MessageProcessing.Core.Models;

/// <summary>
/// مدل پاسخ برای بررسی وضعیت سلامت سیستم (Health Check).
/// این پاسخ از سمت Dispatcher یا سرویس مربوطه برمی‌گردد
/// و نشان‌دهنده وضعیت فعلی سرویس و تعداد کلاینت‌های فعال است.
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// وضعیت فعال یا غیرفعال بودن سرویس.
    /// اگر <c>true</c> باشد یعنی سرویس در حال اجرا و آماده پاسخگویی است.
    /// اگر <c>false</c> باشد یعنی سرویس غیرفعال یا دچار مشکل است.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// تعداد کلاینت‌های فعال که در لحظه بررسی به سیستم متصل هستند.
    /// این عدد برای پایش بار (Load Monitoring) و مانیتورینگ ظرفیت سرویس کاربرد دارد.
    /// </summary>
    public int NumberOfActiveClients { get; set; } = 0;

    /// <summary>
    /// زمان انقضا برای این پاسخ Health Check.
    /// معمولاً به این معناست که وضعیت سلامت تا این زمان معتبر است
    /// و پس از آن باید یک درخواست جدید برای اطمینان از وضعیت سیستم ارسال شود.
    /// </summary>
    public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddMinutes(10);
}

