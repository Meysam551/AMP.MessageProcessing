
using MPS.MessageProcessing.Core.Models;

namespace MPS.MessageProcessing.Core.Interfaces;

public interface IHealthCheckService
{
    Task<HealthCheckResponse> CheckHealthAsync(HealthCheckRequest request);
}
