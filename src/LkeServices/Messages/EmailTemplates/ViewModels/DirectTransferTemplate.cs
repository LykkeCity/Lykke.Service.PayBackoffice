namespace LkeServices.Messages.EmailTemplates.ViewModels
{
    public class DirectTransferTemplate
    {
        public string ClientName { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string ExplorerUrl { get; set; }
        public int Year { get; set; }
    }
}
