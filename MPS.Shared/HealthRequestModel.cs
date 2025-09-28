
namespace MPS.Shared
{
    public class HealthRequestModel
    {
        public string Id { get; set; }
        public DateTime SystemTime { get; set; }
        public int NumberOfConnectedClients { get; set; }
    }
}
