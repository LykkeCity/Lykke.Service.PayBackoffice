using BackOffice.Models;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using System.Collections.Generic;

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
