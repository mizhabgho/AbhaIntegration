using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static readonly string TokenFile = "access_token.txt";
    private static readonly string AuthUrl = "https://dev.abdm.gov.in/api/hiecm/gateway/v3/sessions";

    static async Task Main()
    {
        try
        {
            string? clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            string? clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Console.WriteLine("❌ Error: CLIENT_ID or CLIENT_SECRET environment variables are not set.");
                return;
            }

            string? accessToken = await GetAccessToken(clientId, clientSecret);

            if (!string.IsNullOrEmpty(accessToken))
            {
                File.WriteAllText(TokenFile, accessToken);
                Console.WriteLine($"✅ Access Token saved to {TokenFile}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task<string?> GetAccessToken(string clientId, string clientSecret)
    {
        using (HttpClient client = new HttpClient())
        {
            // ✅ Add Required Headers
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("REQUEST-ID", Guid.NewGuid().ToString()); // Unique per request
            client.DefaultRequestHeaders.Add("TIMESTAMP", DateTime.UtcNow.ToString("o")); // ISO 8601 format
            client.DefaultRequestHeaders.Add("X-CM-ID", "sbx"); // As seen in Postman

            var requestBody = new
            {
                clientId = clientId,
                clientSecret = clientSecret,
                grantType = "client_credentials"  
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"); // ✅ Corrected

            Console.WriteLine($"🔹 Request Body: {jsonBody}");

            HttpResponseMessage response = await client.PostAsync(AuthUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔹 Response: {response.StatusCode} - {responseString}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseData?.accessToken != null)
                {
                    Console.WriteLine($"✅ Access Token: {responseData.accessToken}");
                    return responseData.accessToken;
                }
            }

            Console.WriteLine($"❌ Error: {response.StatusCode} - {responseString}");
            return null;
        }
    }
}
