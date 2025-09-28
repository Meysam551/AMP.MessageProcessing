
using System.Collections.Concurrent;
using MPS.MessageProcessing.Core.Models;

namespace MPS.MessageProcessing.Dispatcher;

//این متد هر بار یک پیام تصادفی تولید می‌کنه
public class MessageQueueSimulator
{
    //ConcurrentQueue تضمین می‌کنه که چند Thread همزمان مشکلی ایجاد نکنه
    private readonly ConcurrentQueue<Message> _queue = new();
    private readonly Random _random = new();

    public async Task<Message> GetNextMessageAsync()
    {
        await Task.Delay(200); // شبیه‌سازی تاخیر 200ms
        var message = new Message
        {
            Id = _random.Next(1, 1000),
            Sender = "Legal",
            Content = GenerateRandomText(10, 50)
        };
        _queue.Enqueue(message);
        return message;
    }

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