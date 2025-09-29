
namespace MPS.MessageProcessing.Core.Models;

/// <summary>
/// مدل پایه برای یک پیام در سیستم پردازش پیام‌ها.
/// این کلاس نماینده پیام خام است که از سمت تولیدکننده (Producer) وارد صف می‌شود
/// و سپس توسط Processorها پردازش خواهد شد.
/// </summary>
public class Message
{
    /// <summary>
    /// شناسه یکتا برای پیام.
    /// این مقدار معمولاً توسط سیستم تولیدکننده یا صف (Queue) تعیین می‌شود
    /// و برای ردیابی و مدیریت پیام‌ها استفاده خواهد شد.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام یا شناسه فرستنده پیام.
    /// می‌تواند یک سرویس، کلاینت، یا حتی Dispatcher باشد
    /// تا مشخص شود پیام از چه منبعی آمده است.
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// محتوای اصلی پیام.
    /// این داده همان چیزی است که Processorها روی آن تحلیل (مثلاً Regex یا Ruleهای دیگر) انجام می‌دهند.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
