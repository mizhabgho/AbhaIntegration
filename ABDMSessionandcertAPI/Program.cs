using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("❌ Error: Please specify 'cert' or 'sess' as an argument.");
            return;
        }

        switch (args[0].ToLower())
        {
            case "cert":
                await RunCertABDM();
                break;
            case "sess":
                await RunSessionABDM();
                break;
            default:
                Console.WriteLine("❌ Error: Invalid argument. Use 'cert' or 'sess'.");
                break;
        }
    }

    private static async Task RunCertABDM()
    {
        Console.WriteLine("🔹 Running CertABDM logic...");
        string ApiUrl = "https://abhasbx.abdm.gov.in/abha/api/v3/profile/public/certificate";

        string accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");

        if (string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("❌ Error: Access token is missing. Run 'sess' first to obtain it.");
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            string requestId = Guid.NewGuid().ToString();
            string timestamp = DateTime.UtcNow.ToString("o");

            using (var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl))
            {
                request.Headers.Add("REQUEST-ID", requestId);
                request.Headers.Add("TIMESTAMP", timestamp);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                HttpResponseMessage response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine("Response Body:");
                Console.WriteLine(responseBody);

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    if (doc.RootElement.TryGetProperty("publicKey", out JsonElement publicKeyElement))
                    {
                        string publicKey = publicKeyElement.GetString();
                    
                        // Save public key as an environment variable
                        Environment.SetEnvironmentVariable("PUBLIC_KEY", publicKey, EnvironmentVariableTarget.User);

                        Console.WriteLine("✅ Public key saved to environment variable 'PUBLIC_KEY'");
                    }
                    else
                    {
                        Console.WriteLine("❌ Error: `publicKey` not found in response.");
                    }
                }
            }
        }
    }

    private static async Task RunSessionABDM()
    {
        Console.WriteLine("🔹 Running SessionABDM logic...");
        string AuthUrl = "https://dev.abdm.gov.in/api/hiecm/gateway/v3/sessions";

        string clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? throw new InvalidOperationException("CLIENT_ID environment variable is not set.");
        string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? throw new InvalidOperationException("CLIENT_SECRET environment variable is not set.");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine("❌ Error: CLIENT_ID or CLIENT_SECRET environment variables are not set.");
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("REQUEST-ID", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("TIMESTAMP", DateTime.UtcNow.ToString("o"));
            client.DefaultRequestHeaders.Add("X-CM-ID", "sbx");

            var requestBody = new
            {
                clientId = clientId,
                clientSecret = clientSecret,
                grantType = "client_credentials"
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(AuthUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔹 Response: {response.StatusCode} - {responseString}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseData?.accessToken != null)
                {
                    string accessToken = responseData.accessToken.ToString();

                    // Save to environment variable
                    Environment.SetEnvironmentVariable("ACCESS_TOKEN", accessToken, EnvironmentVariableTarget.User);

                    Console.WriteLine("✅ Access Token saved to environment variable 'ACCESS_TOKEN'");
                }
            }
            else
            {
                Console.WriteLine($"❌ Error: {response.StatusCode} - {responseString}");
            }
        }
    }
}
