namespace AbhaVerificationApi.Models
{
    public class OtpRequest
    {
        public string[] Scope { get; set; } = { "abha-login", "mobile-verify" };
        public string LoginHint { get; set; } = "mobile";
        public string LoginId { get; set; } // Encrypted mobile number
        public string OtpSystem { get; set; } = "abdm";
    }
}
