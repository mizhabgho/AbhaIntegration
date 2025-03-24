using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Collections.Generic;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    static string configFilePath = "config.json";

    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run <command>");
            return;
        }

        switch (args[0].ToLower())
        {
            case "sendotp":
                await SendOtp();
                break;
            case "verifyotp":
                await VerifyOtp();
                break;
            case "verifyuser":
                await VerifyUser();
                break;
            case "storedata":
                StoreUserData();
                break;
            default:
                Console.WriteLine("❌ Invalid command");
                break;
        }
    }

    // 📌 Store Encrypted Data in config.json
    static void StoreUserData()
    {
        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Enter mobile number");
        Console.WriteLine("2. Enter OTP");
        Console.WriteLine("3. Enter ABHA number");
        Console.WriteLine("4. Enter Aadhar number");
        Console.Write("Choice: ");
        string choice = Console.ReadLine();

        string publicKey = GetPublicKey();

        if (string.IsNullOrEmpty(publicKey))
        {
            Console.WriteLine("❌ Public key not found.");
            return;
        }

        Dictionary<string, string> configData = LoadConfig();

        switch (choice)
        {
            case "1":
                Console.Write("Enter mobile number to encrypt: ");
                string mobile = Console.ReadLine();
                configData["ENCRYPTED_MOBILE_NUMBER"] = RSAEncrypt(mobile, publicKey);
                Console.WriteLine("✅ Mobile number encrypted and stored.");
                break;
            case "2":
                Console.Write("Enter OTP to encrypt: ");
                string otp = Console.ReadLine();
                configData["ENCRYPTED_OTP"] = RSAEncrypt(otp, publicKey);
                Console.WriteLine("✅ OTP encrypted and stored.");
                break;
            case "3":
                Console.Write("Enter ABHA number: ");
                string abha = Console.ReadLine();
                configData["ABHA_NUMBER"] = abha;
                Console.WriteLine("✅ ABHA number stored.");
                break;
            case "4":
                Console.Write("Enter Aadhar number: ");
                string aadhar = Console.ReadLine();
                configData["AADHAR_NUMBER"] = aadhar;
                Console.WriteLine("✅ Aadhar number stored.");
                break;
            default:
                Console.WriteLine("❌ Invalid choice.");
                return;
        }

        SaveConfig(configData);
    }

// 📌 Send OTP API Call (Debugging Mode with Hardcoded Access Token)
    static async Task SendOtp()
    {
        Dictionary<string, string> configData = LoadConfig();

        if (!configData.ContainsKey("ENCRYPTED_MOBILE_NUMBER"))
        {
            Console.WriteLine("❌ Encrypted mobile number not found in config.json.");
            return;
        }

        string encryptedMobile = configData["ENCRYPTED_MOBILE_NUMBER"];

        // 🔥 Hardcoded Access Token (Replace with actual token for debugging)
        string accessToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJBbFJiNVdDbThUbTlFSl9JZk85ejA2ajlvQ3Y1MXBLS0ZrbkdiX1RCdkswIn0.eyJleHAiOjE3NDI4MTc2NzIsImlhdCI6MTc0MjgxNjQ3MiwianRpIjoiZDM0MWM5ZTUtYTEzMi00N2Q0LTg5NmItMDE2NTU3ZDY1OTAxIiwiaXNzIjoiaHR0cHM6Ly9kZXYubmRobS5nb3YuaW4vYXV0aC9yZWFsbXMvY2VudHJhbC1yZWdpc3RyeSIsImF1ZCI6WyJhY2NvdW50IiwiU0JYVElEXzAwNjU3NiJdLCJzdWIiOiJlNmZhMjIyOC1hNTQ0LTQzYWQtYjYzNi1mZGY1NTU2M2VhMWIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJTQlhJRF8wMDkxMjUiLCJzZXNzaW9uX3N0YXRlIjoiYmEzNjE0ZTktNzFlYy00Y2JkLWJlZGMtMTMxYmZmN2I3ZGNjIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwOi8vbG9jYWxob3N0OjkwMDciXSwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIkRJR0lfRE9DVE9SIiwiaGZyIiwiaGl1Iiwib2ZmbGluZV9hY2Nlc3MiLCJoZWFsdGhJZCIsInBociIsIk9JREMiLCJoZWFsdGhfbG9ja2VyIiwiaGlwIiwiSGlkQWJoYVNlYXJjaCIsImhwX2lkIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19LCJTQlhJRF8wMDkxMjUiOnsicm9sZXMiOlsidW1hX3Byb3RlY3Rpb24iXX0sIlNCWFRJRF8wMDY1NzYiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6Im9wZW5pZCBlbWFpbCBwcm9maWxlIiwiY2xpZW50SWQiOiJTQlhJRF8wMDkxMjUiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImNsaWVudEhvc3QiOiIxMDAuNjUuMTYwLjIxMiIsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1zYnhpZF8wMDkxMjUiLCJjbGllbnRBZGRyZXNzIjoiMTAwLjY1LjE2MC4yMTIifQ.idEDF2v-lovDGt0YbH3UiRs4QWH1ALSfvZrWOLSyDZuyx3FRlMsse_QW4he1SD2qfL30QvzVKIVnR1tQBa68xhCWVJs6uj_eYaujbEjJwtZT-qNppdKGltC6JFAoktfgCkenSh3pOriMYJKDBm0QxwNf4FL8LutlQKYtJd1BOiG4o6KLLiD-f_-nTtGtsSlnPukV-efNUdg-8itksvz9VtNZ8teNL4aEPYwlxI0dua265trMlDmBAmkh0io-bAWL8LWQT7OF6gjaxWNhn91oBj-2bBaroU9BF344MwH-wB5VZD26id_Xo9Ky7G5cMP4God_1LLYmbx5vHGdBkB6YcA";
        string requestId = Guid.NewGuid().ToString();
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        Console.WriteLine($"🔒 Encrypted Mobile Number: {encryptedMobile}");
        Console.WriteLine($"🔑 HARD-CODED ACCESS_TOKEN: {accessToken}");
        Console.WriteLine($"📌 REQUEST-ID: {requestId}");
        Console.WriteLine($"📌 TIMESTAMP: {timestamp}");

        var requestData = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = encryptedMobile,
            otpSystem = "abdm"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/request/otp")
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("REQUEST-ID", requestId);
        request.Headers.Add("TIMESTAMP", timestamp);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request);
        string responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"📩 Response: {responseString}");

        // Extract and store txnId
        var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        if (jsonResponse != null && jsonResponse.ContainsKey("txnId"))
        {
            configData["TXN_ID"] = jsonResponse["txnId"];
            SaveConfig(configData);
            Console.WriteLine("✅ txnId stored in config.json");
        }
        else
        {
            Console.WriteLine("❌ Failed to extract txnId from response.");
        }
    }

    // 📌 Verify OTP API Call (Debugging Mode with Hardcoded Access Token)
    static async Task VerifyOtp()
    {
        Dictionary<string, string> configData = LoadConfig();

        if (!configData.ContainsKey("ENCRYPTED_OTP") || !configData.ContainsKey("TXN_ID"))
        {
            Console.WriteLine("❌ Encrypted OTP or TXN_ID not found in config.json.");
            return;
        }

        string encryptedOtp = configData["ENCRYPTED_OTP"];
        string txnId = configData["TXN_ID"];

        // 🔥 Hardcoded Access Token (Replace with actual token for debugging)
        string accessToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJBbFJiNVdDbThUbTlFSl9JZk85ejA2ajlvQ3Y1MXBLS0ZrbkdiX1RCdkswIn0.eyJleHAiOjE3NDI4MTc2NzIsImlhdCI6MTc0MjgxNjQ3MiwianRpIjoiZDM0MWM5ZTUtYTEzMi00N2Q0LTg5NmItMDE2NTU3ZDY1OTAxIiwiaXNzIjoiaHR0cHM6Ly9kZXYubmRobS5nb3YuaW4vYXV0aC9yZWFsbXMvY2VudHJhbC1yZWdpc3RyeSIsImF1ZCI6WyJhY2NvdW50IiwiU0JYVElEXzAwNjU3NiJdLCJzdWIiOiJlNmZhMjIyOC1hNTQ0LTQzYWQtYjYzNi1mZGY1NTU2M2VhMWIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJTQlhJRF8wMDkxMjUiLCJzZXNzaW9uX3N0YXRlIjoiYmEzNjE0ZTktNzFlYy00Y2JkLWJlZGMtMTMxYmZmN2I3ZGNjIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwOi8vbG9jYWxob3N0OjkwMDciXSwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIkRJR0lfRE9DVE9SIiwiaGZyIiwiaGl1Iiwib2ZmbGluZV9hY2Nlc3MiLCJoZWFsdGhJZCIsInBociIsIk9JREMiLCJoZWFsdGhfbG9ja2VyIiwiaGlwIiwiSGlkQWJoYVNlYXJjaCIsImhwX2lkIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19LCJTQlhJRF8wMDkxMjUiOnsicm9sZXMiOlsidW1hX3Byb3RlY3Rpb24iXX0sIlNCWFRJRF8wMDY1NzYiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6Im9wZW5pZCBlbWFpbCBwcm9maWxlIiwiY2xpZW50SWQiOiJTQlhJRF8wMDkxMjUiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImNsaWVudEhvc3QiOiIxMDAuNjUuMTYwLjIxMiIsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1zYnhpZF8wMDkxMjUiLCJjbGllbnRBZGRyZXNzIjoiMTAwLjY1LjE2MC4yMTIifQ.idEDF2v-lovDGt0YbH3UiRs4QWH1ALSfvZrWOLSyDZuyx3FRlMsse_QW4he1SD2qfL30QvzVKIVnR1tQBa68xhCWVJs6uj_eYaujbEjJwtZT-qNppdKGltC6JFAoktfgCkenSh3pOriMYJKDBm0QxwNf4FL8LutlQKYtJd1BOiG4o6KLLiD-f_-nTtGtsSlnPukV-efNUdg-8itksvz9VtNZ8teNL4aEPYwlxI0dua265trMlDmBAmkh0io-bAWL8LWQT7OF6gjaxWNhn91oBj-2bBaroU9BF344MwH-wB5VZD26id_Xo9Ky7G5cMP4God_1LLYmbx5vHGdBkB6YcA";

        string requestId = Guid.NewGuid().ToString();
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        Console.WriteLine($"🔑 HARD-CODED ACCESS_TOKEN: {accessToken}");
        Console.WriteLine($"📌 REQUEST-ID: {requestId}");
        Console.WriteLine($"📌 TIMESTAMP: {timestamp}");
        Console.WriteLine($"🔒 ENCRYPTED OTP: {encryptedOtp}");
        Console.WriteLine($"🔄 TXN_ID: {txnId}");

        var requestData = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            authData = new
            {
                authMethods = new[] { "otp" },
                otp = new
                {
                    txnId = txnId,
                    otpValue = encryptedOtp
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/verify")
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("REQUEST-ID", requestId);
        request.Headers.Add("TIMESTAMP", timestamp);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request);
        string responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"📩 RAW API Response:\n{responseString}\n");

        // Extract and store jwtToken
        var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        if (jsonResponse != null && jsonResponse.ContainsKey("token"))
        {
            configData["JWT_TOKEN"] = jsonResponse["token"];
            SaveConfig(configData);
            Console.WriteLine("✅ jwtToken stored in config.json");
        }
        else
        {
            Console.WriteLine("❌ Failed to extract jwtToken from response.");
        }
    }

    // 📌 Verify User API Call
    static async Task VerifyUser()
    {
        Dictionary<string, string> configData = LoadConfig();

        if (!configData.ContainsKey("ABHA_NUMBER") || !configData.ContainsKey("TXN_ID"))
        {
            Console.WriteLine("❌ ABHA number or TXN_ID not found in config.json.");
            return;
        }

        var requestData = new
        {
            ABHANumber = configData["ABHA_NUMBER"],
            txnId = configData["TXN_ID"]
        };

        var response = await client.PostAsync(
            "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/verify/user",
            new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
        );

        string responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);
    }

    // 📌 Encrypt Data using RSA
    static string RSAEncrypt(string text, string publicKeyPem)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyPem), out _);
            byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    // 📌 Retrieve Public Key from Environment Variables
    static string GetPublicKey()
    {
        return Environment.GetEnvironmentVariable("PUBLIC_KEY", EnvironmentVariableTarget.User);
    }

    // 📌 Load Data from config.json
    static Dictionary<string, string> LoadConfig()
    {
        if (!File.Exists(configFilePath))
            return new Dictionary<string, string>();

        string json = File.ReadAllText(configFilePath);
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }

    // 📌 Save Data to config.json
    static void SaveConfig(Dictionary<string, string> configData)
    {
        string json = JsonConvert.SerializeObject(configData, Formatting.Indented);
        File.WriteAllText(configFilePath, json);
    }
}
