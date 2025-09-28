using Microsoft.AspNetCore.Mvc;
using MPS.Shared;

namespace MPS.MessageProcessing.Management.Controllers
{
    [ApiController]
    [Route("api/module")]
    public class HealthController : ControllerBase
    {
        [HttpPost("health")]
        public IActionResult Post([FromBody] HealthRequestModel request)
        {
            var response = new HealthResponseModel
            {
                IsEnabled = true,
                NumberOfActiveClients = Random.Shared.Next(0, 6),
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            };
            return Ok(response);
        }
    }
}
