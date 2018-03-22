﻿using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class StaffsPageViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
    }
    public class StaffsListViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<EmployeeModel> Staffs { get; set; }
    }
    public class AddStaffDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public string SelectedMerchant { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Id { get; set; }
        public bool IsNewStaff { get; set; }
    }
    public class DeleteStaffDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }

        public string Name { get; set; }
        public string Id { get; set; }
        public string MerchantId { get; set; }
    }
}