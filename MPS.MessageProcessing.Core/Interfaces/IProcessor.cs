
using MPS.MessageProcessing.Core.Models;

namespace MPS.MessageProcessing.Core.Interfaces;

public interface IProcessor
{
    Task<ProcessedMessage> ProcessAsync(Message message, Dictionary<string, string> regexSettings);
}
