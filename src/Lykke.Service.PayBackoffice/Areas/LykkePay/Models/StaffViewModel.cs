using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class StaffViewModel
    {
        public string MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
