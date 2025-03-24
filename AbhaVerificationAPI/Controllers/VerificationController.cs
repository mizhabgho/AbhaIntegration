using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/abha")]
[ApiController]
public class VerificationController : ControllerBase
{
    private readonly IAbhaVerificationService _verificationService;

    public VerificationController(IAbhaVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    // Endpoint to send OTP.
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var response = await _verificationService.SendOtpAsync(request);
        return StatusCode((int)response.StatusCode, response.Content);
    }

    // Endpoint to verify OTP.
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var response = await _verificationService.VerifyOtpAsync(request);
        return StatusCode((int)response.StatusCode, response.Content);
    }

    // Endpoint to verify user.
    [HttpPost("verify-user")]
    public async Task<IActionResult> VerifyUser([FromBody] VerifyUserRequest request)
    {
        var response = await _verificationService.VerifyUserAsync(request);
        return StatusCode((int)response.StatusCode, response.Content);
    }
}
