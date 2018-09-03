using BackOffice.Areas.LykkePay.Models;
using BackOffice.Binders;
using BackOffice.Controllers;
using BackOffice.Helpers;
using BackOffice.Translates;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.BackofficeMembership.Client.Filters;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Lykke.Cqrs;
using Lykke.Service.PayAuth.Client.Models;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayInvoice.Contract;
using Lykke.Service.PayInvoice.Contract.Commands;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayMerchantsView)]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayStaffsView)]
    public class StaffsController : Controller
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly IMapper _mapper;

        private const string ErrorMessageAnchor = "#errorMessage";
        private readonly IEmailPartnerRouterClient _emailPartnerRouterClient;
        protected string PayInvoicePortalResetPasswordLink
               => AzureBinder.PayInvoicePortalResetPasswordLink;
        public StaffsController(
            IPayInvoiceClient payInvoiceClient, 
            IPayAuthClient payAuthClient, 
            IEmailPartnerRouterClient emailPartnerRouterClient, 
            IPayMerchantClient payMerchantClient, 
            ICqrsEngine cqrsEngine, 
            IMapper mapper)
        {
            _payInvoiceClient = payInvoiceClient;
            _payAuthClient = payAuthClient;
            _emailPartnerRouterClient = emailPartnerRouterClient;
            _payMerchantClient = payMerchantClient;
            _cqrsEngine = cqrsEngine;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> StaffsPage(string merchant = "")
        {
            var merchants = await _payMerchantClient.Api.GetAllAsync();

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
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsFull),
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsEdit)
            });
        }
        [HttpPost]
        public async Task<ActionResult> StaffsList(StaffsPageViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.SelectedMerchant))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#selectedMerchant");

            List<StaffViewModel> filteredstaffs;

            if (!string.IsNullOrEmpty(vm.SearchValue))
            {
                var merchants = await _payMerchantClient.Api.GetAllAsync();
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
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsFull),
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayStaffsEdit),
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
            var merchants = await _payMerchantClient.Api.GetAllAsync();
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
        public IActionResult AddOrEditStaff(AddStaffDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.FirstName))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, ErrorMessageAnchor);

            if (string.IsNullOrEmpty(vm.LastName))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, ErrorMessageAnchor);

            if (!vm.Email?.IsValidEmail() ?? true)
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, ErrorMessageAnchor);

            if (vm.IsNewStaff && string.IsNullOrEmpty(vm.Password))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, ErrorMessageAnchor);

            if (vm.IsNewStaff)
            {
                _cqrsEngine.SendCommand(
                    _mapper.Map<RegisterEmployeeCommand>(vm),
                    "lykkepay-employee-registration-ui", 
                    "lykkepay-employee-registration");
            }
            else
            {
                _cqrsEngine.SendCommand(
                    _mapper.Map<UpdateEmployeeCommand>(vm),
                    "lykkepay-employee-registration-ui", 
                    "lykkepay-employee-registration");
            }

            return this.JsonRequestResult("#staffList", Url.Action("StaffsList"),
                new StaffsPageViewModel {SelectedMerchant = vm.SelectedMerchant});
        }

        [HttpPost]
        public async Task<ActionResult> AddOrEditStaffOld(AddStaffDialogViewModel vm)
        {
            try
            {
                return vm.IsNewStaff ? await AddStaffAsync(vm) : await EditStaff(vm);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return this.JsonFailResult("Error: " + ex.InnerException.Message, ErrorMessageAnchor);
                return this.JsonFailResult("Error: " + ex.Message, ErrorMessageAnchor);
            }
        }

        private async Task<ActionResult> AddStaffAsync(AddStaffDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.FirstName))
                return this.JsonFailResult("FirstName required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.LastName))
                return this.JsonFailResult("LastName required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.Password))
                return this.JsonFailResult("Password required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.Email))
                return this.JsonFailResult("Email required", ErrorMessageAnchor);

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(vm.Email);
            if (!match.Success)
                return this.JsonFailResult("Email not valid", ErrorMessageAnchor);

            var employees = await _payInvoiceClient.GetEmployeesAsync(vm.SelectedMerchant);
            if (employees != null && employees.Select(x => x.Email)
                    .Contains(vm.Email, StringComparer.OrdinalIgnoreCase))
            {
                return this.JsonFailResult(Phrases.AlreadyExists, ErrorMessageAnchor);
            }

            EmployeeModel employee = await _payInvoiceClient.AddEmployeeAsync(new CreateEmployeeModel
            {
                Email = vm.Email,
                LastName = vm.LastName,
                FirstName = vm.FirstName,
                MerchantId = vm.SelectedMerchant
            });

            await _payAuthClient.RegisterAsync(new RegisterModel
            {
                Email = vm.Email,
                EmployeeId = employee.Id,
                MerchantId = vm.SelectedMerchant,
                Password = vm.Password
            });

            ResetPasswordTokenModel passwordResetToken = await _payAuthClient.CreateResetPasswordTokenAsync(
                new CreateResetPasswordTokenRequest
                {
                    MerchantId = employee.MerchantId,
                    EmployeeId = employee.Id
                });

            await ResetPasswordAsync(new ResetPasswordModel
            {
                Email = vm.Email,
                EmailTemplate = "lykkepay_employee_registration",
                FullName = vm.FirstName,
                Password = vm.Password,
                PasswordResetToken = passwordResetToken.Id
            });

            return this.JsonRequestResult("#staffList", Url.Action("StaffsList"),
                new StaffsPageViewModel() {SelectedMerchant = vm.SelectedMerchant});
        }

        private async Task<ActionResult> EditStaff(AddStaffDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.FirstName))
                return this.JsonFailResult("FirstName required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.LastName))
                return this.JsonFailResult("LastName required", ErrorMessageAnchor);

            EmployeeModel employee = await _payInvoiceClient.GetEmployeeAsync(vm.Id);

            await _payInvoiceClient.UpdateEmployeeAsync(new UpdateEmployeeModel
            {
                Email = employee.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Id = vm.Id,
                MerchantId = vm.SelectedMerchant,
                IsBlocked = vm.IsBlocked
            });

            if (!string.IsNullOrEmpty(vm.Password))
            {
                await _payAuthClient.UpdateAsync(new UpdateCredentialsModel
                {
                    Email = employee.Email,
                    EmployeeId = vm.Id,
                    MerchantId = vm.SelectedMerchant,
                    Password = vm.Password
                });

                ResetPasswordTokenModel passwordResetToken = await _payAuthClient.CreateResetPasswordTokenAsync(
                    new CreateResetPasswordTokenRequest
                    {
                        MerchantId = employee.MerchantId,
                        EmployeeId = employee.Id
                    });

                await ResetPasswordAsync(new ResetPasswordModel
                {
                    Email = employee.Email,
                    EmailTemplate = "lykkepay_password_reset",
                    FullName = vm.FirstName,
                    Password = vm.Password,
                    PasswordResetToken = passwordResetToken.Id
                });
            }

            return this.JsonRequestResult("#staffList", Url.Action("StaffsList"),
                new StaffsPageViewModel {SelectedMerchant = vm.SelectedMerchant});
        }

        private Task ResetPasswordAsync(ResetPasswordModel model)
        {
            var payload = new Dictionary<string, string>
            {
                {"UserName", model.FullName},
                {"ResetPasswordUrl", string.Format(PayInvoicePortalResetPasswordLink, model.PasswordResetToken)},
                {"Year", DateTime.Today.Year.ToString()}
            };

            return _emailPartnerRouterClient.Send(new SendEmailCommand
            {
                EmailAddresses = new[] {model.Email},
                Template = model.EmailTemplate,
                Payload = payload
            });
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
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmDeleteStaff");
            await _payInvoiceClient.DeleteEmployeeAsync(vm.Id);
            return this.JsonRequestResult("#StaffsPage", Url.Action("StaffsPage"), vm.MerchantId);
        }
    }
}
