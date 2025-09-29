
using System.Collections.Concurrent;
using MPS.MessageProcessing.Core.Models;

namespace MPS.MessageProcessing.Dispatcher;

/// <summary>
/// شبیه‌ساز یک صف پیام (Message Queue) ساده.
/// این کلاس برای تست و توسعه استفاده می‌شود تا بدون نیاز به اتصال به سیستم‌های صف واقعی
/// (مثل RabbitMQ یا Kafka) بتوان پیام‌های آزمایشی تولید و پردازش کرد.
/// </summary>
public class MessageQueueSimulator
{
    // صف thread-safe برای نگهداری پیام‌ها
    private readonly ConcurrentQueue<Message> _queue = new();

    // تولیدکننده اعداد تصادفی برای تولید شناسه‌ها و متن‌های آزمایشی
    private readonly Random _random = new();

    /// <summary>
    /// دریافت پیام بعدی از صف (به‌صورت Async).
    /// این متد با تأخیر مصنوعی (Delay) پیام جدیدی تولید کرده
    /// و آن را در صف قرار می‌دهد تا رفتار یک منبع پیام واقعی شبیه‌سازی شود.
    /// </summary>
    /// <returns>یک نمونه <see cref="Message"/> با محتوای تصادفی</returns>
    public async Task<Message> GetNextMessageAsync()
    {
        // شبیه‌سازی تأخیر شبکه یا صف واقعی
        await Task.Delay(200);

        var message = new Message
        {
            Id = _random.Next(1, 1000),      // تولید شناسه تصادفی برای پیام
            Sender = "Legal",                // فرستنده پیش‌فرض (برای تست)
            Content = GenerateRandomText(10, 50) // متن تصادفی
        };

        _queue.Enqueue(message);
        return message;
    }

    /// <summary>
    /// تولید یک رشته متنی تصادفی با طول مشخص.
    /// کاراکترها از حروف کوچک انگلیسی و فاصله انتخاب می‌شوند.
    /// </summary>
    /// <param name="minLength">حداقل طول متن</param>
    /// <param name="maxLength">حداکثر طول متن</param>
    /// <returns>رشته متنی تصادفی</returns>
    private string GenerateRandomText(int minLength, int maxLength)
    {
        int length = _random.Next(minLength, maxLength);
        const string chars = "abcdefghijklmnopqrstuvwxyz ";
        char[] buffer = new char[length];

        for (int i = 0; i < length; i++)
            buffer[i] = chars[_random.Next(chars.Length)];

        return new string(buffer);
    }
}
