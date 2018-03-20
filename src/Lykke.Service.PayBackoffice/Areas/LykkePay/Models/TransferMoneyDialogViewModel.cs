using BackOffice.Models;
using Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class TransferMoneyDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public LykkePayWalletListSettings Wallets { get; set; }
        public string SelectedWallet { get; set; }
        public string SelectedMerchant { get; set; }
        public List<RequestTransferModel> SelectedPaymentRequests { get; set; }
    }
    public class TransferModel
    {
        public string SelectedMerchant { get; set; }
        public List<RequestTransferModel> SelectedPaymentRequests { get; set; }
    }
}
