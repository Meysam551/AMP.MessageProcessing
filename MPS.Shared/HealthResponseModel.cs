
namespace MPS.Shared
{
    public class HealthResponseModel
    {
        public bool IsEnabled { get; set; } = true;
        public int NumberOfActiveClients { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
