using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models.Supervisors
{
    public class AddSupervisorDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string SelectedMerchant { get; set; }
        public string SelectedEmployee { get; set; }
        public string[] SelectedMerchants { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public IReadOnlyList<EmployeeModel> Employees { get; set; }
    }
}
