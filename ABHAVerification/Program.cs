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

        string authorizationToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJBbFJiNVdDbThUbTlFSl9JZk85ejA2ajlvQ3Y1MXBLS0ZrbkdiX1RCdkswIn0.eyJleHAiOjE3NDI1NTU4NjksImlhdCI6MTc0MjU1NDY2OSwianRpIjoiODBlMjViNmItNGZjYS00N2YyLWEyYzktM2VkOWVlNTAwZjZmIiwiaXNzIjoiaHR0cHM6Ly9kZXYubmRobS5nb3YuaW4vYXV0aC9yZWFsbXMvY2VudHJhbC1yZWdpc3RyeSIsImF1ZCI6WyJhY2NvdW50IiwiU0JYVElEXzAwNjU3NiJdLCJzdWIiOiJlNmZhMjIyOC1hNTQ0LTQzYWQtYjYzNi1mZGY1NTU2M2VhMWIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJTQlhJRF8wMDkxMjUiLCJzZXNzaW9uX3N0YXRlIjoiZGE5YTg4NDUtOGM3YS00YzFkLWI4NDctNTE3ZTlmNDJkN2M3IiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwOi8vbG9jYWxob3N0OjkwMDciXSwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIkRJR0lfRE9DVE9SIiwiaGZyIiwiaGl1Iiwib2ZmbGluZV9hY2Nlc3MiLCJoZWFsdGhJZCIsInBociIsIk9JREMiLCJoZWFsdGhfbG9ja2VyIiwiaGlwIiwiSGlkQWJoYVNlYXJjaCIsImhwX2lkIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19LCJTQlhJRF8wMDkxMjUiOnsicm9sZXMiOlsidW1hX3Byb3RlY3Rpb24iXX0sIlNCWFRJRF8wMDY1NzYiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6Im9wZW5pZCBlbWFpbCBwcm9maWxlIiwiY2xpZW50SWQiOiJTQlhJRF8wMDkxMjUiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImNsaWVudEhvc3QiOiIxMDAuNjUuMTYwLjIxMiIsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1zYnhpZF8wMDkxMjUiLCJjbGllbnRBZGRyZXNzIjoiMTAwLjY1LjE2MC4yMTIifQ.TWoJGGSqn7j4aGPxLJgQIU_jaZkvC2gkCxqiRoNQBMojNpBA5SQF068SdPSLfklv_4r0kTfDFa5lGPlnOP8ljSQkNCvoQ7jSbLjl7zT--N5ETf17xhOuTsTd6NmEIAKFwzs_pqXyycVeO-0fBhg0qZv_N8qbId4HN7CLrsDPbYmEabnPWfJsY2yHpoX75OuZmKGJ-dmTO4ExDQKn4C_iXzhSQ8wtVVwNtJnil1om_HTKgbRDw7j-8E5LbyWrQrASNAK17J5unNqwfLKqSm0RGN4_u1wFr6CNkf3AuRNpixy9dtcoRl4JnSLEx9B1JevMxjPJT2fcvjn_Hvh1-9VZyg";
        //retreive the authorization token from the environment variable which is acces_token env variable
        //string? authorizationToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");

        var requestBody = new
        {
            scope = new[] { "abha-login", "mobile-verify" },
            loginHint = "mobile",
            loginId = "VYlLmxxIcz/1xv4kBo5pi7toIBI3Etzx/O4y5k4EwRxrnAU2JQt29I83kz/Vwtizg5gewAWAYTYsfueRv7hgQJtlYdFeo2IsL0V0ZJhhFRiLF8SlYqncIve1EFVGxHbCXMnzR5ZylTQH1dLIG5JMnw6XqnpsJ1LQz8opxM/RgmrwPR/u7aiJpcTjR4CecxT4BQ5Uh+GgLqDHF1W3N6/TSwDrUQdnar2jX2rphTng9tmR4PSiUtUtU+bsI/SMrssKCBnk2fWiVbMHn4j56Vp3wcoak3rNuX8gTOafAUCibPUS8aFGFvwfk9kpjd3bE6OiVXNZshqJm8b+JAQmR2terDhmdN9dJYR8LoDLsC1SlTmw/oZ1Mbs3ShyDSJ/Msv5UlZMs/6jD6CgbKA5jqUUo3TjEc92hVrzOozlhwaP4G28YUiLlx8DQT6ZGx1JdGkJu1/NgSugL4CY7gWYOWqQ4W0iyGflYxmxyqa5wYSNEqtrYPlEXIo8KcAHYiMxOIyjjggHFsHSI/g50rzmyjIFQR+LUYlhMNUlYxXcCd5jJvcAHSX4QWyru4scfM9E9GlbUKz/5lv8K8tv2C83alj2uh8sUyFSxi1wbD9kmQqwP8ELbY2qrdLu28DjwiGR0LDQqRaOdtI4jVMb0ynnzWA9O7+JEtKaC+etsmRZbGAorVpU=",
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
