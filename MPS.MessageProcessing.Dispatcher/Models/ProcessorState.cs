
using Grpc.Core;
using MessageProcessing.Dispatcher.Grpc;

namespace MPS.MessageProcessing.Dispatcher.Models;

public class ProcessorState
{
    public IServerStreamWriter<MessageToProcess> Stream { get; set; }
    public DateTime LastRequestTime { get; set; }
    public bool IsActive { get; set; }
}

