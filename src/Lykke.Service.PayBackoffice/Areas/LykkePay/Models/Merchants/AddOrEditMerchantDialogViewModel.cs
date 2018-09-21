using BackOffice.Models;

namespace BackOffice.Areas.LykkePay.Models
{
    public class AddOrEditMerchantDialogViewModel : IPersonalAreaDialog
    {
        public bool IsNewMerchant { get; set; }
        public string Caption { get; set; }
        public string Width { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ApiKey { get; set; }
        public string LwId { get; set; }
        public string LogoImage { get; set; }
        public string Email { get; set; }
    }
    public class DeleteMerchantDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }

        public string Name { get; set; }
        public string Id { get; set; }
    }
}
