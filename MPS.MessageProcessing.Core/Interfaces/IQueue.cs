
namespace MPS.MessageProcessing.Core.Interfaces;

public interface IQueue<T>
{
    Task EnqueueAsync(T item);
    Task<T?> DequeueAsync();
    Task<int> CountAsync();
}
