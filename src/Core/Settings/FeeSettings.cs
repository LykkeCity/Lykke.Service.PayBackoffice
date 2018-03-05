namespace Core.Settings
{
    public class FeeSettings
    {
        public TargetClientIdFeeSettings TargetClientId { get; set; }
    }

    public class TargetClientIdFeeSettings
    {
        public string BankCard { get; set; }
        public string CashOut { get; set; }
        public string Hft { get; set; }
    }
}
