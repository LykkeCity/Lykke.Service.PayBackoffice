namespace Core.Ethereum.Models
{
    public class GetContractModel
    {
        public string Contract { get; set; }
    }

    public class OperationResponse
    {
        public string OperationId { get; set; }
    }

    public class AdapBalanceResponseResponse
    {
        public string  CoinAdaperAddress{ get; set; }
        public string UserAddress { get; set; }
        public decimal Balance { get; set; }
    }

    public class EstimationResponse
    {
        public bool IsAllowed { get; set; }
        public string GasAmount { get; set; }
    }
}
