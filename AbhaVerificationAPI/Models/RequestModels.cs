public class SendOtpRequest
{
    // Encrypted mobile number for OTP generation.
    public string EncryptedMobileNumber { get; set; }
}

public class VerifyOtpRequest
{
    // Transaction ID from send OTP response.
    public string TxnId { get; set; }
    
    // Encrypted OTP value.
    public string EncryptedOtp { get; set; }
}

public class VerifyUserRequest
{
    // ABHA number to verify.
    public string ABHANumber { get; set; }
    
    // Transaction ID associated with the session.
    public string TxnId { get; set; }
}
