using Lykke.Backoffice.Common;
using System.Collections.Generic;

namespace BackOffice.Settings
{
    public class SupportedBrowsersSettings
    {
        public IList<Browser> Browsers { get; set; }
        public string[] SkipUrls { get; set; }
    }
}
