using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Lykke.Service.PayInternal.Client;
using BackOffice.Filters;
using Core.Users;
using BackOffice.Controllers;
using BackOffice.Translates;
using Lykke.Service.PayInvoice.Client;
using BackOffice.Areas.LykkePay.Models;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using System.Text.RegularExpressions;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using BackOffice.Binders;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayStaffsView)]
    public class StaffsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayAuthClient _payAuthClient;
        private const string ErrorMessageAnchor = "#errorMessage";
        private readonly IEmailPartnerRouterClient _emailPartnerRouterClient;
        protected string PayInvoicePortalResetPasswordLink
               => AzureBinder.PayInvoicePortalResetPasswordLink;
        public StaffsController(
            IPayInternalClient payInternalClient, IPayInvoiceClient payInvoiceClient, IPayAuthClient payAuthClient, IEmailPartnerRouterClient emailPartnerRouterClient)
        {
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient;
            _payAuthClient = payAuthClient;
            _emailPartnerRouterClient = emailPartnerRouterClient;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> StaffsPage(string merchant = "")
        {
            var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();

            if (!string.IsNullOrEmpty(merchant) && !merchants.Select(x => x.Id).Contains(merchant))
            {
                return this.JsonFailResult(Phrases.InvalidValue, "#merchant");
            }

            if (merchants.Any())
            {
                if (string.IsNullOrEmpty(merchant))
                {
                    merchant = merchants.Select(x => x.Id).First();
                }
            }

            return View(new StaffsPageViewModel
            {
                SelectedMerchant = merchant,
                Merchants = merchants,
                IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsFull),
                IsEditAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsEdit)
            });
        }
        [HttpPost]
        public async Task<ActionResult> StaffsList(StaffsPageViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.SelectedMerchant))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#selectedMerchant");

            var filteredstaffs = new List<StaffViewModel>();
            if (!string.IsNullOrEmpty(vm.SearchValue))
            {
                var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();
                var allstaffs = await _payInvoiceClient.GetEmployeesAsync();
                filteredstaffs = allstaffs.Where(s => !string.IsNullOrEmpty(s.Email) && s.Email.Contains(vm.SearchValue)).Select(x => new StaffViewModel()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    MerchantId = x.MerchantId,
                    MerchantName = merchants.FirstOrDefault(w=>w.Id == x.MerchantId)?.DisplayName
                }).ToList();
            }
            else
            {
                var staffs = await _payInvoiceClient.GetEmployeesAsync(vm.SelectedMerchant);
                filteredstaffs = staffs.Select(x => new StaffViewModel()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    MerchantId = vm.SelectedMerchant
                }).ToList();
            }
            var viewModel = new StaffsListViewModel
            {
                Employees = filteredstaffs,
                SelectedMerchant = vm.SelectedMerchant,
                IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsFull),
                IsEditAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsEdit),
                IsSearchMode = !string.IsNullOrEmpty(vm.SearchValue)
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditStaffDialog(string id = null, string merchant = null)
        {
            var employee = new EmployeeModel();
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(merchant))
            {
                employee = await _payInvoiceClient.GetEmployeeAsync(id);
            }
            var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();
            var viewmodel = new AddStaffDialogViewModel()
            {
                SelectedMerchant = merchant,
                Merchants = merchants,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Id = employee.Id,
                IsNewStaff = id == null,
                IsBlocked = employee.IsBlocked
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditStaff(AddStaffDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.FirstName))
                return this.JsonFailResult("FirstName required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.LastName))
                return this.JsonFailResult("LastName required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.Email))
                return this.JsonFailResult("Email required", ErrorMessageAnchor);

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(vm.Email);
            if (!match.Success)
                return this.JsonFailResult("Email not valid", ErrorMessageAnchor);

            var employee = new EmployeeModel();
            var employees = await _payInvoiceClient.GetEmployeesAsync(vm.SelectedMerchant);
            try
            {
                if (vm.IsNewStaff)
                {
                    if (employees != null && employees.Select(x => x.Email).Contains(vm.Email))
                    {
                        return this.JsonFailResult(Phrases.AlreadyExists, ErrorMessageAnchor);
                    }
                    if (string.IsNullOrEmpty(vm.Password))
                        return this.JsonFailResult("Password required", ErrorMessageAnchor);

                    employee = await _payInvoiceClient.AddEmployeeAsync(new CreateEmployeeModel()
                    {
                        Email = vm.Email,
                        LastName = vm.LastName,
                        FirstName = vm.FirstName
                    });
                }
                else
                {
                    await _payInvoiceClient.UpdateEmployeeAsync(new UpdateEmployeeModel()
                    {
                        Email = vm.Email,
                        FirstName = vm.FirstName,
                        LastName = vm.LastName,
                        Id = vm.Id,
                        MerchantId = vm.SelectedMerchant,
                        IsBlocked = vm.IsBlocked
                    });
                }
                if (!string.IsNullOrEmpty(vm.Password))
                {
                    var rm = new RegisterModel();
                    rm.Email = vm.Email;
                    rm.EmployeeId = string.IsNullOrEmpty(vm.Id) ? employee.Id : vm.Id;
                    rm.MerchantId = vm.SelectedMerchant;
                    rm.Password = vm.Password;
                    if (vm.IsNewStaff)
                        await _payAuthClient.RegisterAsync(rm);
                    else
                    {
                        var updatemodel = new UpdateCredentialsModel()
                        {
                            Email = vm.Email,
                            EmployeeId = vm.Id,
                            MerchantId = vm.SelectedMerchant,
                            Password = vm.Password
                        };
                        await _payAuthClient.UpdateAsync(updatemodel);
                    }
                    var payload = new Dictionary<string, string>
                        {
                            {"UserEmail", vm.Email},
                            {"ClientFullName", vm.FirstName},
                            {"ResetLink", PayInvoicePortalResetPasswordLink},
                            {"DateTime", DateTime.Now.ToString()},
                            {"Year", DateTime.Today.Year.ToString()}
                        };
                    var emails = new List<string>();
                    emails.Add(vm.Email);
                    await _emailPartnerRouterClient.Send(new SendEmailCommand
                    {
                        EmailAddresses = emails.ToArray(),
                        Template = "PasswordResetTemplate",
                        Payload = payload
                    });
                }
                return this.JsonRequestResult("#staffList", Url.Action("StaffsList"), new StaffsPageViewModel() { SelectedMerchant = vm.SelectedMerchant });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return this.JsonFailResult("Error: " + ex.InnerException.Message, ErrorMessageAnchor);
                return this.JsonFailResult("Error: " + ex.Message, ErrorMessageAnchor);
            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteStaffDialog(string merchant, string id)
        {
            var employee = await _payInvoiceClient.GetEmployeeAsync(id);
            var viewModel = new DeleteStaffDialogViewModel
            {
                Caption = "Delete employee",
                Name = string.Format("{0} {1}", employee.FirstName, employee.LastName),
                Id = id,
                MerchantId = merchant
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteStaff(DeleteStaffDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.Id))
            {
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmDeleteStaff");
            }
            await _payInvoiceClient.DeleteEmployeeAsync(vm.Id);

            return this.JsonRequestResult("#StaffsPage", Url.Action("StaffsPage"), vm.MerchantId);
        }
    }
}
