namespace BackOffice.Settings
{
    public class BackOfficeTwoFactorVerificationSettings
    {
        public BackOfficeTwoFactorVerificationSettings()
        {
            UseVerification = false;
        }

        public bool UseVerification { get; set; }
    }
}
