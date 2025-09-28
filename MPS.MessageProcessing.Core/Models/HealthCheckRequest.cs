
namespace MPS.MessageProcessing.Core.Models;

public class HealthCheckRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime SystemTime { get; set; } = DateTime.UtcNow;
    public int NumberOfConnectedClients { get; set; }
}