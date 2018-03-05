namespace LkeServices.Messages.EmailTemplates.ViewModels
{
    public class OrdinaryCashOutDoneTemplate
    {
        public string AssetId { get; set; }
        public double? Amount { get; set; }
        public string ExplorerUrl { get; set; }
        public int Year { get; set; }
        public int ValidDays { get; set; }
    }
}
