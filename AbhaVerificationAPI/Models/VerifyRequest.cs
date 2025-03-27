namespace AbhaIntegration.Models
{
    public class VerifyRequest
    {
        public required string ABHANumber { get; set; }  // ✅ Ensure it's always provided
        public required string TxnId { get; set; }       // ✅ Ensure it's always provided
    }
}
