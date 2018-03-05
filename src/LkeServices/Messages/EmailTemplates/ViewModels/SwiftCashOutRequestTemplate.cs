namespace LkeServices.Messages.EmailTemplates.ViewModels
{
    public class SwiftCashOutRequestTemplate
    {
        public string FullName { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string Bic { get; set; }
        public string AccNum { get; set; }
        public string AccName { get; set; }
        public string BankName { get; set; }
        public string AccHolderAddress { get; set; }
        public int Year { get; set; }
        public string ConfirmUrl { get; set; }
        public string DeclineUrl { get; set; }
    }
}