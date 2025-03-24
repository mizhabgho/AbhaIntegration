using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AbhaVerificationApi.Models;

namespace AbhaVerificationApi.Services
{
    public class VerifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _abhaBaseUrl = "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login";

        public VerifyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private void AddCommonHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("REQUEST-ID", Guid.NewGuid().ToString());
            request.Headers.Add("TIMESTAMP", DateTime.UtcNow.ToString("o"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "your_api_token_here");
        }

        public async Task<string> RequestOtp(OtpRequest otpRequest)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_abhaBaseUrl}/request/otp")
            {
                Content = new StringContent(JsonSerializer.Serialize(otpRequest), Encoding.UTF8, "application/json")
            };
            AddCommonHeaders(request);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> VerifyOtp(OtpVerifyRequest otpVerifyRequest)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_abhaBaseUrl}/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(otpVerifyRequest), Encoding.UTF8, "application/json")
            };
            AddCommonHeaders(request);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> VerifyAbhaUser(AbhaVerifyRequest abhaVerifyRequest, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_abhaBaseUrl}/verify/user")
            {
                Content = new StringContent(JsonSerializer.Serialize(abhaVerifyRequest), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            AddCommonHeaders(request);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
