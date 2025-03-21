using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main()
    {
        string url = "https://abhasbx.abdm.gov.in/abha/api/v3/profile/login/request/otp";
        
        string requestId = Guid.NewGuid().ToString();
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        string authorizationToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJBbFJiNVdDbThUbTlFSl9JZk85ejA2ajlvQ3Y1MXBLS0ZrbkdiX1RCdkswIn0.eyJleHAiOjE3NDI1NTgwODksImlhdCI6MTc0MjU1Njg4OSwianRpIjoiZmU0NGZmYmYtYTc2NC00NDkwLTg4NjctNWVjYjBjODJkYjcxIiwiaXNzIjoiaHR0cHM6Ly9kZXYubmRobS5nb3YuaW4vYXV0aC9yZWFsbXMvY2VudHJhbC1yZWdpc3RyeSIsImF1ZCI6WyJhY2NvdW50IiwiU0JYVElEXzAwNjU3NiJdLCJzdWIiOiJlNmZhMjIyOC1hNTQ0LTQzYWQtYjYzNi1mZGY1NTU2M2VhMWIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJTQlhJRF8wMDkxMjUiLCJzZXNzaW9uX3N0YXRlIjoiNDJkZjk4MmItNzAwNS00M2JkLWI1NGYtZTcyYzk3YTk0OWQ2IiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwOi8vbG9jYWxob3N0OjkwMDciXSwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIkRJR0lfRE9DVE9SIiwiaGZyIiwiaGl1Iiwib2ZmbGluZV9hY2Nlc3MiLCJoZWFsdGhJZCIsInBociIsIk9JREMiLCJoZWFsdGhfbG9ja2VyIiwiaGlwIiwiSGlkQWJoYVNlYXJjaCIsImhwX2lkIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19LCJTQlhJRF8wMDkxMjUiOnsicm9sZXMiOlsidW1hX3Byb3RlY3Rpb24iXX0sIlNCWFRJRF8wMDY1NzYiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6Im9wZW5pZCBlbWFpbCBwcm9maWxlIiwiY2xpZW50SWQiOiJTQlhJRF8wMDkxMjUiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImNsaWVudEhvc3QiOiIxMDAuNjUuMTYwLjIxMiIsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1zYnhpZF8wMDkxMjUiLCJjbGllbnRBZGRyZXNzIjoiMTAwLjY1LjE2MC4yMTIifQ.Uj9MhImk0ASUzr8EFZaxBwbmhHFvJ4JYtZbCXotyH8Cnk3Zg7u66EEwrS4jIBoFiFm4aLLet_JuH2padFs4sCORDkYeQ6T1gZI5tjMnExs-Wn9p7jwpGONU5-JNbSfp3SVntcLxK0Ov073a_-segSQQPl17fDvr1TGm2d9fd3U0B_hxjmWWJNpjIdZ5rgIBIJqf4jTQIfuIfLGE7fwSUWrZZP77IvtL1GxrUYexSGVtegFYbyBIqtVNClYiltHv1lEuma5AScjYyr15mFd3sHUVKmma_XFi2EQWYqh_g9ep9ytqWu7sAutqdDd6D4fUKS740iPMz67U3Y-FGBLglfQ";
        //retreive the authorization token from the environment variable which is acces_token env variable
        //string? authorizationToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");

        var requestBody = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = "VP+yXwxA+ppl5ktgO6tnofgjyXMHHjV3GG4HSMokL62xKTZJAznpR2524F2tQfS5DtRHca7NZi8KVX+C4HOvJbFdTY1URG8TemZTiIF5SjHmBzBvrkNFHJ29X/Z4IkajTKkP8XCMp6BskxOwGfycy4o5RIJvSxw1Mb/CSfg/79qdJd1V/yHx7rRqXTQcMfSpXFZLQB8Dpu3vPDoaMPzQtZ8OnH3jgy4+Id4iuPKEaKl7XiKuGv2BJSlJ2Q/Od5CFKyA/rRcGpouqAu3esiy+wZz7xPaQ/IMuWfXrKpZ+pPcZwZEglGT6v6i3j+39Hjatc4/kQxjrzv8CQ1AytXyvh0wQPwQ3s54vZ1mZocmx89kCYKy4m4f5fNsEyaQVkwrDi1K2sspFiDLKe4rXtnemPSR/b93yLr0OTXlzwD+j7pYbH+dupKq2WJ863JotHDoLiBvyOYGLheQP5gXdQQVqmqEHrLmpx6znKY5j6Ehv6uuNwl4Tr4wUq/xLs8I5Mcz1M2g7uoIdBWtiR5ejrXJT+OaO/Xt9LzDbHxrPf59is4KYj9liQE+HBJmJXe4YHW1PNJmgiid7FstK40ylWjbkEIKr0kWLig+2LRRg6Q2llj1GVmtNSMXOKSlhtzDqMR4zm8PYI2cUBppJqjCRQsVWxovr5249yHag7cB7EZUrZXU=",
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

            // Extract txnId and store it in an environment variable
            var jsonResponse = JObject.Parse(responseString);
            if (jsonResponse["txnId"] != null)
            {
                string txnId = jsonResponse["txnId"].ToString();
                Environment.SetEnvironmentVariable("TXN_ID", txnId);
                Console.WriteLine($"Transaction ID stored in environment variable: {txnId}");
            }
            else
            {
                Console.WriteLine("txnId not found in response.");
            }
        }
    }
}
