
namespace MPS.MessageProcessing.Core.Models;

public class HealthCheckResponse
{
    public bool IsEnabled { get; set; } = true;
    public int NumberOfActiveClients { get; set; } = 0;
    public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddMinutes(10);
}
