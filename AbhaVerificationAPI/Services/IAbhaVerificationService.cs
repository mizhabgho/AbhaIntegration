using System.Threading.Tasks;
using RestSharp;

public interface IAbhaVerificationService
{
    Task<RestResponse> SendOtpAsync(SendOtpRequest request);
    Task<RestResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<RestResponse> VerifyUserAsync(VerifyUserRequest request);
}
