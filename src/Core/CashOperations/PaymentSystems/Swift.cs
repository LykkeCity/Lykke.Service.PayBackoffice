namespace Core.CashOperations.PaymentSystems
{
    public interface ISwiftFields
    {
        string Bic { get; }
        string AccNumber { get; }
        string AccName { get; }
        string AccHolderAddress { get; }
        string BankName { get; }


        string AccHolderCountry { get; }
        string AccHolderZipCode { get; }
        string AccHolderCity { get; }
    }

    public class Swift : ISwiftFields
    {
        public string Bic { get; set; }
        public string AccNumber { get; set; }
        public string AccName { get; set; }
        public string AccHolderAddress { get; set; }
        public string BankName { get; set; }

        public string AccHolderCountry { get; set; }
        public string AccHolderZipCode { get; set; }
        public string AccHolderCity { get; set; }
    }
}
