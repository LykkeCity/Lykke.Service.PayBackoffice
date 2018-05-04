using Lykke.SettingsReader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Settings
{
    public class EmailPartnerRouterServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
