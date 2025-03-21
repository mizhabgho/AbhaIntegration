using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main()
    {
        string url = "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/request/otp";
        
        // Generate unique request ID and timestamp
        string requestId = Guid.NewGuid().ToString(); // Generates a new GUID
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601 format

        string authorizationToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJBbFJiNVdDbThUbTlFSl9JZk85ejA2ajlvQ3Y1MXBLS0ZrbkdiX1RCdkswIn0.eyJleHAiOjE3NDI1MzI5MjQsImlhdCI6MTc0MjUzMTcyNCwianRpIjoiMDQ2MDFjNzAtNzBlMS00MTg1LWI3MTUtMTIyNTk2NmNkYmM5IiwiaXNzIjoiaHR0cHM6Ly9kZXYubmRobS5nb3YuaW4vYXV0aC9yZWFsbXMvY2VudHJhbC1yZWdpc3RyeSIsImF1ZCI6WyJhY2NvdW50IiwiU0JYVElEXzAwNjU3NiJdLCJzdWIiOiJlNmZhMjIyOC1hNTQ0LTQzYWQtYjYzNi1mZGY1NTU2M2VhMWIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJTQlhJRF8wMDkxMjUiLCJzZXNzaW9uX3N0YXRlIjoiYzVkMjVkZDktZDE0ZC00NzI2LThhMDYtODE3MTA1NDQ5NWJjIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwOi8vbG9jYWxob3N0OjkwMDciXSwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIkRJR0lfRE9DVE9SIiwiaGZyIiwiaGl1Iiwib2ZmbGluZV9hY2Nlc3MiLCJoZWFsdGhJZCIsInBociIsIk9JREMiLCJoZWFsdGhfbG9ja2VyIiwiaGlwIiwiSGlkQWJoYVNlYXJjaCIsImhwX2lkIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19LCJTQlhJRF8wMDkxMjUiOnsicm9sZXMiOlsidW1hX3Byb3RlY3Rpb24iXX0sIlNCWFRJRF8wMDY1NzYiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6Im9wZW5pZCBlbWFpbCBwcm9maWxlIiwiY2xpZW50SWQiOiJTQlhJRF8wMDkxMjUiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImNsaWVudEhvc3QiOiIxMDAuNjUuMTYwLjIxMiIsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1zYnhpZF8wMDkxMjUiLCJjbGllbnRBZGRyZXNzIjoiMTAwLjY1LjE2MC4yMTIifQ.N3b2OOPJlphCiinr0Hq1ArIhK_EJPAUyANlrqhnyR-QcJgKXHtfbAIXWs_v1x61fp2OXXwOSamI4kb7hX_A6KOBLiPChTg2ADTElc1KnOSSdpEzA_9E0pC81RlN656bmbawoo2zYiVLT5OiPnsgWr98xWbC6FqXuGieGzGJWtTcIaARn4c8C71sI07IujJnOwKAQ0ExzHs94Hl7G3xXJpK3HF8nSNgY-CxeqRJliow65xRvVqE_Ma8s0e28oAORM8R3LPQPxU3wYnZP0DWR3hjnp3DnSfzEGQM7LLldN0DFrVibpJjLqsPoduf8IA_BEmMY8UEnVkxo3C0Jo0-o9XA"; // Replace with a valid token

        var requestBody = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = "V/Ojlp/3x173uHwhGCOw5og+HfsAmuWz4o2w/jvb7gH3rJYe/4FgD2b/+ZF1p5/XKbwJ54OBq8aSwUhQIQz8ZaH5SRZLZHddzcVhh79YWKBSWnDlxe6hRNT1K+hEZgb+Dl6340/DYokJOURm6HTiKmdQxhmw9xo6JZpvXrj7ymLJstmgVM8qrGPfp/y5lA3I4jfi4WVV0Iam22gqCMzCiImeyBEjAGK3dHVve+AIx8Axq4vDRrTry3kIFxG1VIKDa+M6eIXTEuyaspc1opVXMLCeG7Ghi7PhM+0G/zLu/rihz84+J38ZXDu609xJJwzASRCIEKYaGji1d/rZOv7K7Q1fn6u8qKQch1y3/8ElYqJ15KPx+k7ExifzwG/gRIcVM9XkaNEqy6N1YAI4nicvBnmRzjlO6ZxBQY2mZDKw/kVLVtDBRwpHkcsSSMu29OsBtK82wDo7jkxXMW7BRG5FJzx9s4FDLZedkjFkAohxgmd4hGOOJjsDGmJBO6QRcwWQJg4pdJYaiLJqh+3iBR7KuL5kgAWt/pzMr1UCtCXVbKbyzPBEE9iK/ge6yHm3m4CQEZrNfvP5eU+wmWM2UHFWTcdg8Edz2gJHkemhRVyAcK6SGmGHZqaz096IEYylbu7AL7SibAMqC5+8xJFhGPUlTdmxXb9qkRD4Mj4c6kRdtGk=",
            otpSystem = "abdm"
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("REQUEST-ID", requestId);
            client.DefaultRequestHeaders.Add("TIMESTAMP", timestamp);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);

            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response:");
            Console.WriteLine(responseString);
        }
    }
}
