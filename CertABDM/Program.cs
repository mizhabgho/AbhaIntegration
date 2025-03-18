using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string TokenFile = "access_token.txt"; // File containing the access token
    private static readonly string PublicKeyFile = "publickey_token.txt"; // File to save public key
    private static readonly string ApiUrl = "https://abhasbx.abdm.gov.in/abha/api/v3/profile/public/certificate";

    static async Task Main()
    {
        try
        {
            // Read the access token from the file
            if (!File.Exists(TokenFile))
            {
                Console.WriteLine("❌ Error: Access token file not found.");
                return;
            }

            string accessToken = File.ReadAllText(TokenFile).Trim();
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("❌ Error: Access token is empty.");
                return;
            }

            string requestId = Guid.NewGuid().ToString(); // Generates a new GUID
            string timestamp = DateTime.UtcNow.ToString("o"); // Generates an ISO 8601 timestamp

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

                // Parse JSON response
                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    if (doc.RootElement.TryGetProperty("publicKey", out JsonElement publicKeyElement))
                    {
                        string publicKey = publicKeyElement.GetString();
                        
                        // Save to file
                        File.WriteAllText(PublicKeyFile, publicKey);
                        Console.WriteLine($"✅ Public key saved to {PublicKeyFile}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Error: `publicKey` not found in response.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
