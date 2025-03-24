namespace AbhaVerificationApi.Models
{
    public class OtpVerifyRequest
    {
        public string[] Scope { get; set; } = { "abha-login", "mobile-verify" };
        public AuthData AuthData { get; set; }
    }

    public class AuthData
    {
        public string[] AuthMethods { get; set; } = { "otp" };
        public OtpDetails Otp { get; set; }
    }

    public class OtpDetails
    {
        public string TxnId { get; set; }
        public string OtpValue { get; set; } // Encrypted OTP
    }
}
