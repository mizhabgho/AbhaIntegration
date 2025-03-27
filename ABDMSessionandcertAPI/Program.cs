using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("❌ Error: Please specify 'cert' or 'sess' as an argument.");
            return;
        }

        switch (args[0].ToLower())
        {
            case "sess":
                await RunSessionABDM();
                break;
            case "cert":
                await RunCertABDM();
                break;
            default:
                Console.WriteLine("❌ Error: Invalid argument. Use 'cert' or 'sess'.");
                break;
        }
    }

    private static async Task RunSessionABDM()
    {
        Console.WriteLine("🔹 Running SessionABDM logic...");
        string AuthUrl = "https://dev.abdm.gov.in/api/hiecm/gateway/v3/sessions";

        string clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine("❌ Error: CLIENT_ID or CLIENT_SECRET environment variables are not set.");
            return;
        }

        try
        {
            var requestBody = new
            {
                clientId = clientId,
                clientSecret = clientSecret,
                grantType = "client_credentials"
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("REQUEST-ID", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("TIMESTAMP", DateTime.UtcNow.ToString("o"));
            client.DefaultRequestHeaders.Add("X-CM-ID", "sbx");

            HttpResponseMessage response = await client.PostAsync(AuthUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔹 Response: {response.StatusCode} - {responseString}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                string accessToken = responseData?.accessToken;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    Environment.SetEnvironmentVariable("accesstoken", accessToken, EnvironmentVariableTarget.User);
                    Console.WriteLine("✅ Access Token saved to environment variable 'accesstoken'");
                }
            }
            else
            {
                Console.WriteLine($"❌ Error: {response.StatusCode} - {responseString}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception: {ex.Message}");
        }
    }

    private static async Task RunCertABDM()
    {
        Console.WriteLine("🔹 Running CertABDM logic...");
        string ApiUrl = "https://abhasbx.abdm.gov.in/abha/api/v3/profile/public/certificate";

        // Try reading token from file if environment variable is missing
        string accessToken = Environment.GetEnvironmentVariable("accesstoken", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(accessToken) && File.Exists("access_token.txt"))
        {
            accessToken = File.ReadAllText("access_token.txt").Trim();
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("❌ Error: Access token is missing. Run 'sess' first to obtain it.");
            return;
        }

        try
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("REQUEST-ID", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("TIMESTAMP", DateTime.UtcNow.ToString("o"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            Console.WriteLine($"🔹 Using Access Token: {accessToken}");

            HttpResponseMessage response = await client.GetAsync(ApiUrl);
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine("Response Body:");
            Console.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Error: API Request Failed. Check credentials.");
                return;
            }

            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                if (doc.RootElement.TryGetProperty("publicKey", out JsonElement publicKeyElement))
                {   
                    string publicKey = publicKeyElement.GetString();
                    Environment.SetEnvironmentVariable("publicKey", publicKey, EnvironmentVariableTarget.User);

                    Console.WriteLine("✅ Public key saved to environment variable 'publicKey'");
                }
                else
                {
                    Console.WriteLine("❌ Error: `publicKey` not found in response.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception: {ex.Message}");
        }
    }
}
