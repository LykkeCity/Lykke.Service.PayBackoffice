using Lykke.Service.BackofficeMembership.Client.AutorestClient.Models;
using Lykke.Service.BackofficeMembership.Client.Models;

namespace BackOffice.Models
{
    public class IndexModel
    {
        public string LangId { get; set; }
    }

    public class IndexPageModel
    {
        public BrowserSessionModel BrowserSession { get; set; }
        public string GoogleApiClientId { get; set; }
        public string Test { get; set; }
    }

    public class AuthenticateModel
    {
        public string GoogleSignInIdToken { get; set; }
        public string Code { get; set; }
    }


    public class MainMenuViewModel
    {
        public string Ver { get; set; }
        public UserRolesPair UserRolesPair { get; set; }
    }

    public class TwoFactorInfo
    {
        public bool ExistCode { get; set; }
        public string ImageUrl { get; set; }
        public string TextKey { get; set; }
        public bool UseVerification { get; set; }
    }

    public class WalletComboboxViewModel
    {
        public string ClientId { get; set; }
        public string WalletId { get; set; }
        public string RedirectUrl { get; set; }
    }
}
