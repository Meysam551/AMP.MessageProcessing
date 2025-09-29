
using Grpc.Core;
using MPS.MessageProcessingProto.Dispatcher.Grpc;

namespace MPS.MessageProcessing.Dispatcher.Models;

/// <summary>
/// وضعیت یک پردازشگر پیام (Processor) متصل به Dispatcher.
/// این کلاس اطلاعات مربوط به اتصال، جریان ارسال پیام‌ها و زمان آخرین فعالیت را نگه‌داری می‌کند.
/// </summary>
public class ProcessorState
{
    /// <summary>
    /// جریان ارتباطی gRPC برای ارسال پیام‌ها به Processor.
    /// از طریق این Stream، Dispatcher پیام‌ها را به پردازشگر ارسال می‌کند.
    /// </summary>
    public IServerStreamWriter<ProtoMessageToProcess> Stream { get; set; } = null!;

    /// <summary>
    /// وضعیت فعال/غیرفعال بودن پردازشگر.
    /// اگر <c>true</c> باشد پردازشگر در حال پردازش پیام‌ها است،
    /// در غیر این صورت به حالت انتظار یا غیرفعال درآمده است.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// زمان آخرین درخواست یا فعالیتی که پردازشگر انجام داده است.
    /// Dispatcher از این مقدار برای شناسایی پردازشگرهای غیرفعال (Idle/Dead) استفاده می‌کند.
    /// </summary>
    public DateTime LastRequestTime { get; set; }
}


