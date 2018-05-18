using BackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class RefundMoneyDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string SelectedMerchant { get; set; }
        public string SelectedPaymentRequest { get; set; }
        public string SelectedWalletAddress { get; set; }
        public IReadOnlyList<string> Wallets { get; set; }
        public string SelectedWallet { get; set; }
        public string ManualWalletAddress { get; set; }
    }
    public class RefundModel
    {
        public string SelectedMerchant { get; set; }
        public string SelectedPaymentRequest { get; set; }
        public string SelectedWalletAddress { get; set; }
        public string Wallets { get; set; }
    }
}
