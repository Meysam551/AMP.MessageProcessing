
namespace MPS.MessageProcessing.Core.Models;

/// <summary>
/// مدل درخواست برای بررسی وضعیت سلامت سیستم (Health Check).
/// این کلاس توسط Dispatcher یا سایر اجزای سیستم استفاده می‌شود
/// تا اطلاعات لحظه‌ای وضعیت سرویس جمع‌آوری و بررسی شود.
/// </summary>
public class HealthCheckRequest
{
    /// <summary>
    /// شناسه یکتا برای درخواست Health Check.
    /// به صورت پیش‌فرض مقدار آن یک GUID جدید است تا
    /// هر درخواست از درخواست دیگر متمایز باشد.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// زمان فعلی سیستم در لحظه ایجاد درخواست.
    /// این فیلد کمک می‌کند تا تفاوت زمانی بین کلاینت و سرور
    /// در حین بررسی سلامت اندازه‌گیری شود.
    /// </summary>
    public DateTime SystemTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تعداد کلاینت‌های متصل در لحظه ثبت این Health Check.
    /// این مقدار برای مانیتورینگ بار سیستم و کنترل ظرفیت پردازشگرها استفاده می‌شود.
    /// </summary>
    public int NumberOfConnectedClients { get; set; }
}
