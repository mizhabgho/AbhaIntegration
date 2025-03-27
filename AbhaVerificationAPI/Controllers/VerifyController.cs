using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AbhaIntegration.Services;

namespace AbhaIntegration.Controllers
{
    [ApiController]
    [Route("api/verify")]
    public class VerifyController : ControllerBase
    {
        private readonly VerifyService _verifyService;

        public VerifyController(VerifyService verifyService)
        {
            _verifyService = verifyService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp()
        {
            try
            {
                string response = await _verifyService.SendOtpRequest();
                return Ok(new { message = "OTP sent successfully", response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
