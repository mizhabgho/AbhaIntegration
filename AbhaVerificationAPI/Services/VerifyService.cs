using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbhaIntegration.Services
{
    public class VerifyService
    {
        private readonly HttpClient _httpClient;
        private readonly dynamic _config;

        public VerifyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            string configPath = "C:\\GitHub\\AbhaIntegration\\AbhaEncryptionConsole\\config.json";
            _config = JsonConvert.DeserializeObject(File.ReadAllText(configPath));
        }

        private string GetEncryptedMobileNumber()
        {
            string encryptedMobile = _config?.ENCRYPTED_MOBILE_NUMBER;

            if (string.IsNullOrEmpty(encryptedMobile))
            {
                Console.WriteLine("⚠️ ERROR: Encrypted mobile number not found in config.json!");
                throw new Exception("Encrypted mobile number is missing in the configuration.");
            }

            Console.WriteLine($"📞 ENCRYPTED_MOBILE_NUMBER: {encryptedMobile}");
            return encryptedMobile;
        }

        private string GetAccessToken()
        {
            // Try getting from environment variable
            string accessToken = Environment.GetEnvironmentVariable("accesstoken", EnvironmentVariableTarget.User);

            // If missing, check access_token.txt file
            if (string.IsNullOrEmpty(accessToken) && File.Exists("access_token.txt"))
            {
                accessToken = File.ReadAllText("access_token.txt").Trim();
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("❌ Error: Access token is missing. Run 'sess' first to obtain it.");
            }

            Console.WriteLine($"🔑 Using Access Token: {accessToken}");
            return accessToken;
        }



        public async Task<string> SendOtpRequest()
        {
            string encryptedMobile = GetEncryptedMobileNumber();
            string accessToken = GetAccessToken();
            string requestId = Guid.NewGuid().ToString();
            string timestamp = DateTime.UtcNow.ToString("o");

            var requestBody = new
            {
                scope = new[] { "abha-login", "mobile-verify" },
                loginHint = "mobile",
                loginId = encryptedMobile,
                otpSystem = "abdm"
            };
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("REQUEST-ID", requestId);
            _httpClient.DefaultRequestHeaders.Add("TIMESTAMP", timestamp);

            Console.WriteLine("📡 Sending OTP request...");
            HttpResponseMessage response = await _httpClient.PostAsync("https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/request/otp", content);

            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("🔄 Response: " + responseBody);

            var jsonResponse = JObject.Parse(responseBody);
            string txnId = jsonResponse["txnId"]?.ToString();
            Console.WriteLine("📌 Transaction ID: " + txnId);

            return responseBody;
        }
    }
}
