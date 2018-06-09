namespace Core.Settings
{
    public class DbSettings
    {
        public string LogsConnString { get; set; }
    }

    public class LykkePayWalletSettings
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class LykkePayWalletListSettings
    {
        public LykkePayWalletSettings[] Wallets { get; set; }
    }

    public class GoogleAuthSettings
    {
        public string ApiClientId { get; set; }

        public string AvailableEmailsRegex { get; set; }

        public string DefaultAdminEmail { get; set; }
    }
    
    public class SlackIntegrationSettings
    {
        public class Channel
        {
            public string Type { get; set; }
            public string WebHookUrl { get; set; }
        }

        public string Env { get; set; }
        public Channel[] Channels { get; set; }
    }


}
