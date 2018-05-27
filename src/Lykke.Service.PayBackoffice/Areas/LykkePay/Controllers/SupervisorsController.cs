using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInternal.Client;
using Microsoft.AspNetCore.Authorization;
using BackOffice.Controllers;
using BackOffice.Translates;
using Lykke.Service.PayInvoice.Client;
using BackOffice.Areas.LykkePay.Models.Supervisors;
using Lykke.Service.PayInternal.Client.Models.Supervisor;

namespace Lykke.Service.PayBackoffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    public class SupervisorsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private const string ErrorMessageAnchor = "#errorMessage";
        public SupervisorsController(
            IPayInternalClient payInternalClient,
            IPayInvoiceClient payInvoiceClient)
        {
            _payInvoiceClient = payInvoiceClient;
            _payInternalClient = payInternalClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SupervisorsPage(string merchant = "")
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

            return View(new SupervisorsPageViewModel
            {
                SelectedMerchant = merchant,
                Merchants = merchants
            });
        }

        [HttpPost]
        public async Task<ActionResult> SupervisorsList(SupervisorsPageViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.SelectedMerchant))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#selectedMerchant");

            var staffs = await _payInvoiceClient.GetEmployeesAsync(vm.SelectedMerchant);
            var filteredstaffs = new List<SupervisorViewModel>();
            foreach (var staff in staffs)
            {
                try
                {
                    var supervisor = await _payInternalClient.GetSupervisingMerchantsAsync(vm.SelectedMerchant, staff.Id);
                    if (supervisor != null)
                    {
                        var item = new SupervisorViewModel()
                        {
                            Id = staff.Id,
                            Email = staff.Email,
                            FirstName = staff.FirstName,
                            LastName = staff.LastName,
                            SupervisingMerchants = supervisor.Merchants
                        };
                        filteredstaffs.Add(item);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            var viewModel = new SupervisorsListViewModel
            {
                Employees = filteredstaffs,
                SelectedMerchant = vm.SelectedMerchant
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<ActionResult> AddSupervisorDialog(string merchant = null)
        {
            var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();
            var employees = await _payInvoiceClient.GetEmployeesAsync(merchant);
            var viewmodel = new AddSupervisorDialogViewModel()
            {
                SelectedMerchant = merchant,
                Merchants = merchants,
                Employees = employees
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddSupervisor(AddSupervisorDialogViewModel vm)
        {
            var request = new CreateSupervisingEmployeeRequest();
            request.EmployeeId = vm.SelectedEmployee;
            request.MerchantId = vm.SelectedMerchant;
            request.SupervisorMerchants = string.Join(";", vm.SelectedMerchants);
            await _payInternalClient.SetSupervisingMerchantsAsync(request);
            return this.JsonRequestResult("#supervisorsList", Url.Action("SupervisorsList"), new SupervisorsPageViewModel() { SelectedMerchant = vm.SelectedMerchant });
        }
    }
}
