using Lykke.GoogleAuthenticator;

namespace BackOffice.Settings
{
    public class TwoFactorVerificationSettingsEx : TwoFactorVerificationSettings
    {
        public TwoFactorVerificationSettingsEx()
        {
            TrustedTimeSpan = "01:00:00";
        }

        public string TrustedTimeSpan { get; set; }
    }
}
