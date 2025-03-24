using System;
using System.Threading.Tasks;
using RestSharp;

public class AbhaVerificationService : IAbhaVerificationService
{
    // Base URL for the ABHA endpoints.
    private const string BaseUrl = "https://abhasbx.abdm.gov.in/abha/api/v3/profile";

    // Create a RestClient instance. This is safe as a singleton.
    private readonly RestClient _client = new RestClient(BaseUrl);

    // Retrieve tokens from environment variables.
    private string AccessToken => Environment.GetEnvironmentVariable("ACCESS_TOKEN");
    private string PublicKey => Environment.GetEnvironmentVariable("PUBLIC_KEY");

    // Add common headers for all requests.
    private void AddCommonHeaders(RestRequest request)
    {
        request.AddHeader("REQUEST-ID", Guid.NewGuid().ToString());
        request.AddHeader("TIMESTAMP", DateTime.UtcNow.ToString("o"));
        request.AddHeader("Authorization", $"Bearer {AccessToken}");
        request.AddHeader("Content-Type", "application/json");
    }

    public async Task<RestResponse> SendOtpAsync(SendOtpRequest request)
    {
        var restRequest = new RestRequest("/login/request/otp", Method.Post);
        AddCommonHeaders(restRequest);

        restRequest.AddJsonBody(new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = request.EncryptedMobileNumber,
            otpSystem = "abdm"
        });

        return await _client.ExecuteAsync(restRequest);
    }

    public async Task<RestResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var restRequest = new RestRequest("/login/verify", Method.Post);
        AddCommonHeaders(restRequest);

        restRequest.AddJsonBody(new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            authData = new
            {
                authMethods = new[] { "otp" },
                otp = new
                {
                    txnId = request.TxnId,
                    otpValue = request.EncryptedOtp
                }
            },
            otpSystem = "abdm"
        });

        return await _client.ExecuteAsync(restRequest);
    }

    public async Task<RestResponse> VerifyUserAsync(VerifyUserRequest request)
    {
        var restRequest = new RestRequest("/login/verify/user", Method.Post);
        AddCommonHeaders(restRequest);

        restRequest.AddJsonBody(new
        {
            ABHANumber = request.ABHANumber,
            txnId = request.TxnId
        });

        return await _client.ExecuteAsync(restRequest);
    }
}
