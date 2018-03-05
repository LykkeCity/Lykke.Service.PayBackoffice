namespace Core.Offchain.Models
{
    public class OffchainResult
    {
        public string TransferId { get; set; }
        public string TransactionHex { get; set; }
        public OffchainOperationResult OperationResult { get; set; }
    }
}
