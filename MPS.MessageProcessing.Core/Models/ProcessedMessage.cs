
namespace MPS.MessageProcessing.Core.Models;

/// <summary>
/// مدل نماینده‌ی یک پیام پردازش‌شده توسط Processor.
/// هر پیام پس از پردازش (مثلاً بررسی Regex یا اعتبارسنجی) در قالب این مدل
/// به Dispatcher بازگردانده می‌شود.
/// </summary>
public class ProcessedMessage
{
    /// <summary>
    /// شناسه پیام پردازش‌شده.
    /// این مقدار باید با شناسه پیام اصلی (Message.Id) مطابقت داشته باشد
    /// تا Dispatcher بتواند نتیجه را به پیام اولیه مرتبط کند.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام یا نوع موتور پردازش (Engine) که این پیام را پردازش کرده است.
    /// برای مثال: "RegexEngine" یا "MLModelEngine".
    /// این مقدار برای ردیابی و تست چندین Processor مختلف اهمیت دارد.
    /// </summary>
    public string Engine { get; set; } = string.Empty;

    /// <summary>
    /// طول محتوای پیام (تعداد کاراکترها).
    /// این مقدار برای تحلیل‌های ساده آماری یا بررسی کیفیت داده‌ها به کار می‌رود.
    /// </summary>
    public int MessageLength { get; set; }

    /// <summary>
    /// نتیجه کلی اعتبارسنجی پیام.
    /// اگر پردازش موفقیت‌آمیز باشد true خواهد بود،
    /// در غیر این صورت (مثلاً خطا یا عدم انطباق با قوانین) false خواهد بود.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// نتایج بررسی‌های دینامیک (Dynamic Regex).
    /// کلید: نام الگو (مثلاً "ContainsNumber")
    /// مقدار: نتیجه بررسی (true اگر منطبق بود، false در غیر این صورت).
    /// این بخش امکان توسعه آسان قوانین پردازش بدون تغییر کد اصلی را فراهم می‌کند.
    /// </summary>
    public Dictionary<string, bool> RegexResults { get; set; } = new();
}
