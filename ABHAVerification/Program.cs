using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.Write("Enter your mobile number: ");
        string mobileNumber = Console.ReadLine();

        // Get Public Key and Access Token from Environment
        string publicKey = Environment.GetEnvironmentVariable("PUBLIC_KEY");
        string accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");

        if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("Error: Missing environment variables. Ensure PUBLIC_KEY and ACCESS_TOKEN are set.");
            return;
        }

        // Ensure public key is in correct PEM format
        string formattedPublicKey = FormatPublicKey(publicKey);

        // Encrypt Mobile Number
        string encryptedMobile = EncryptMobileNumber(mobileNumber, formattedPublicKey);
        if (encryptedMobile == null)
        {
            Console.WriteLine("Encryption failed.");
            return;
        }

        Console.WriteLine($"Encrypted Mobile Number: {encryptedMobile}");

        // Send OTP Request
        await SendOtpRequest(encryptedMobile, accessToken);
    }

    static string FormatPublicKey(string key)
    {
        key = key.Trim();

        if (key.StartsWith("-----BEGIN PUBLIC KEY-----") && key.EndsWith("-----END PUBLIC KEY-----"))
        {
            return key;
        }

        key = key.Replace("\\n", "\n").Replace("\r", "").Trim();

        if (!key.Contains("\n"))
        {
            key = Regex.Replace(key, ".{64}", "$0\n");
        }

        if (!key.StartsWith("-----BEGIN PUBLIC KEY-----"))
        {
            key = "-----BEGIN PUBLIC KEY-----\n" + key;
        }
        if (!key.EndsWith("-----END PUBLIC KEY-----"))
        {
            key += "\n-----END PUBLIC KEY-----";
        }

        return key;
    }

    static string EncryptMobileNumber(string mobileNumber, string publicKeyPem)
    {
        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem.ToCharArray());

            byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(mobileNumber), RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Encryption error: " + ex.Message);
            return null;
        }
    }

    static async Task SendOtpRequest(string encryptedMobile, string accessToken)
    {
        using HttpClient client = new HttpClient();

        var requestData = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = encryptedMobile,
            otpSystem = "abdm"
        };

        string jsonPayload = JsonSerializer.Serialize(requestData);
        Console.WriteLine("\nSending OTP request...");

        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/request/otp")
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("REQUEST-ID", Guid.NewGuid().ToString());
            request.Headers.Add("TIMESTAMP", DateTime.UtcNow.ToString("o"));

            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"\nResponse Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending OTP request: {ex.Message}");
        }
    }
}
