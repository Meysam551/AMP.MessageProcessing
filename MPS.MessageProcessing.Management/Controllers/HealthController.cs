using Microsoft.AspNetCore.Mvc;
using MPS.Shared;

namespace MPS.MessageProcessing.Management.Controllers
{
    /// <summary>
    /// کنترلر HealthCheck برای سامانه مدیریت.
    /// این کنترلر درخواست‌های HealthCheck از Dispatcher یا Processor را دریافت و پاسخ می‌دهد.
    /// </summary>
    [ApiController]
    [Route("api/module")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// نقطه پایان HealthCheck
        /// </summary>
        /// <param name="request">شیء شامل اطلاعات HealthCheck ارسال شده از Dispatcher/Processor</param>
        /// <returns>شیء <see cref="HealthResponseModel"/> شامل وضعیت سامانه</returns>
        [HttpPost("health")]
        public IActionResult Post([FromBody] HealthRequestModel request)
        {
            // تولید پاسخ HealthCheck
            var response = new HealthResponseModel
            {
                IsEnabled = true,                                // سامانه همواره فعال است
                NumberOfActiveClients = Random.Shared.Next(0, 6), // تعداد تصادفی پردازش‌کننده‌های فعال
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)  // زمان انقضا (ده دقیقه بعد)
            };

            // بازگرداندن پاسخ با HTTP 200 OK
            return Ok(response);
        }
    }
}
