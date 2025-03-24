using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AbhaVerificationApi.Services;
using AbhaVerificationApi.Models;

namespace AbhaVerificationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerifyController : ControllerBase
    {
        private readonly VerifyService _verifyService;

        public VerifyController(VerifyService verifyService)
        {
            _verifyService = verifyService;
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request)
        {
            var response = await _verifyService.RequestOtp(request);
            return Ok(new { message = "OTP sent successfully", response });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request)
        {
            var response = await _verifyService.VerifyOtp(request);
            return Ok(new { message = "OTP verified successfully", response });
        }

        [HttpPost("verify-abha")]
        public async Task<IActionResult> VerifyAbha([FromBody] AbhaVerifyRequest request, [FromHeader] string token)
        {
            var response = await _verifyService.VerifyAbhaUser(request, token);
            return Ok(new { message = "ABHA verified successfully", response });
        }
    }
}
