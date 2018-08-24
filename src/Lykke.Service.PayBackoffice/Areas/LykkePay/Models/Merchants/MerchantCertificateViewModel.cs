using BackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class MerchantCertificateViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string MerchantId { get; set; }
        public string MerchantDisplayName { get; set; }
    }

    public class MerchantRsaKeys
    {
        public string Private { get; set; }
        public string Public { get; set; }
    }
}
