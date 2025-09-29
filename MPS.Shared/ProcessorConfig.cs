
namespace MPS.Shared;

/// <summary>
/// تنظیمات پیکربندی مربوط به یک Processor.
/// این کلاس معمولاً از سمت Dispatcher به Processor ارسال می‌شود
/// تا قوانین پردازش (مثلاً Regexهای مختلف) به‌صورت داینامیک تنظیم شوند.
/// </summary>
public class ProcessorConfig
{
    /// <summary>
    /// مجموعه الگوهای Regex که Processor باید روی محتوای پیام‌ها اعمال کند.
    /// کلید: نام یا شناسه الگو (مثلاً "ContainsNumber" یا "ContainsHello").
    /// مقدار: رشته Regex که محتوای پیام با آن تست خواهد شد.
    /// 
    /// این ساختار امکان تغییر و اضافه کردن قوانین جدید را بدون نیاز به تغییر کد اصلی فراهم می‌کند.
    /// </summary>
    public Dictionary<string, string> RegexSettings { get; set; } = new();
}

