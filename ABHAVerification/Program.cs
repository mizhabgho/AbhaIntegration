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

        // Retrieve Public Key and Access Token from environment variables
        string publicKey = Environment.GetEnvironmentVariable("PUBLIC_KEY");
        string accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");

        // Ensure that the required environment variables are set
        if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("Error: Missing environment variables. Ensure PUBLIC_KEY and ACCESS_TOKEN are set.");
            return;
        }

        // Print Access Token and Public Key for debugging
        Console.WriteLine($"\n[DEBUG] Access Token: {accessToken}");
        Console.WriteLine($"\n[DEBUG] Public Key (Before Formatting):\n{publicKey}");

        // Format the public key to ensure correct PEM structure
        string formattedPublicKey = FormatPublicKey(publicKey);

        // Print the formatted public key
        Console.WriteLine($"\n[DEBUG] Public Key (After Formatting):\n{formattedPublicKey}");

        // Encrypt the mobile number using RSA with OAEP-SHA1 padding
        string encryptedMobile = EncryptMobileNumber(mobileNumber, formattedPublicKey);
        if (encryptedMobile == null)
        {
            Console.WriteLine("Encryption failed.");
            return;
        }

        // Print Encrypted Mobile Number
        Console.WriteLine($"\n[DEBUG] Encrypted Mobile Number: {encryptedMobile}");

        // Store encrypted mobile number in an environment variable (optional)
        Environment.SetEnvironmentVariable("ENCRYPTED_MOBILE", encryptedMobile);

        // Send OTP request using the encrypted mobile number
        await SendOtpRequest(encryptedMobile, accessToken);
    }

    /// <summary>
    /// Ensures the public key is correctly formatted as a PEM string.
    /// </summary>
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

    /// <summary>
    /// Encrypts the mobile number using RSA with ECB/OAEPWithSHA-1AndMGF1Padding.
    /// </summary>
    static string EncryptMobileNumber(string mobileNumber, string publicKeyPem)
    {
        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem.ToCharArray());

            // Encrypt using RSA/ECB/OAEPWithSHA-1AndMGF1Padding
            byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(mobileNumber), RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(encryptedBytes); // Convert encrypted bytes to Base64 string
        }
        catch (Exception ex)
        {
            Console.WriteLine("Encryption error: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends an OTP request to the API using the encrypted mobile number.
    /// </summary>
    static async Task SendOtpRequest(string encryptedMobile, string accessToken)
    {
        using HttpClient client = new HttpClient();

        var requestData = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = encryptedMobile, // Encrypted mobile number in API body
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

            // Add required headers
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("REQUEST-ID", Guid.NewGuid().ToString());
            request.Headers.Add("TIMESTAMP", DateTime.UtcNow.ToString("o"));

            // Send the request
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
