﻿using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using System.Collections.Generic;

namespace BackOffice.Areas.LykkePay.Models
{
    public class InvoicesListViewModel : PagedListModel
    {
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public IReadOnlyList<InvoiceViewModel> Invoices { get; set; }
        public string SelectedMerchant { get; set; }
        public bool IsEditAccess { get; set; }
        public bool IsFullAccess { get; set; }
    }
    public class InvoiceViewModel
    {
        public InvoiceModel Invoice { get; set; }
        public PaymentRequestModel Request { get; set; }
    }
}
