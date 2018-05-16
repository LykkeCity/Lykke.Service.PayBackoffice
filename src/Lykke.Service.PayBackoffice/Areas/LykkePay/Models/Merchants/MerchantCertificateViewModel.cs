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
        public string DisplayName { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public string OrgUnit { get; set; }
    }
    public class MerchantCertificate
    {
        public string Private { get; set; }
        public string Public { get; set; }
    }
}
