using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Offchain
{
    public class OffchainConstants
    {
        public const decimal BtcMinCashout = 3.0M;
        public const decimal LkkMinCashout = 50000M;

        public const decimal BtcDefaultCashout = 10.0M;
        public const decimal LkkDefaultCashout = 1000000M;

        public const string HubCashoutSettingsKey = "AutoHubCashout";
    }
}
