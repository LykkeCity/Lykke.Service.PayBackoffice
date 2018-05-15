using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class InvoicePaymentRequestsViewModel : PagedListModel
    {
        public string InvoiceId { get; set; }
        public List<PaymentRequestModel> Requests { get; set; }
    }
}
