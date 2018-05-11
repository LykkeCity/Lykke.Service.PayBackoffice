using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackOffice.Filters;
using Core.Users;
using BackOffice.Controllers;
using BackOffice.Areas.LykkePay.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using PagedList.Core;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayBackoffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayMerchantsView)]
    public class InvoicesController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private const string ErrorMessageAnchor = "#errorMessage";

        public InvoicesController(
            IPayInternalClient payInternalClient,
            IPayInvoiceClient payInvoiceClient)
        {
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> InvoicesPage()
        {
            var model = new InvoicesListViewModel();
            model.Merchants = await _payInternalClient.GetMerchantsAsync();
            model.CurrentPage = 1;
            model.IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> InvoicesList(InvoicesListViewModel vm)
        {
            var invoices = await _payInvoiceClient.GetMerchantInvoicesAsync(vm.SelectedMerchant);
            var invoiceslist = new List<InvoiceViewModel>();
            foreach (var invoice in invoices)
            {
                var model = new InvoiceViewModel();
                model.Invoice = invoice;
                try
                {
                    model.Request = await _payInternalClient.GetPaymentRequestAsync(vm.SelectedMerchant, invoice.PaymentRequestId);
                    invoiceslist.Add(model);
                }
                catch(Exception ex)
                {

                }
            }
            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);
            
            
            var pagedlist = new List<InvoiceViewModel>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)invoiceslist.Count / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (invoiceslist.Count != 0)
                pagedlist = invoiceslist.AsQueryable().ToPagedList(currentPage, vm.PageSize).ToList();
            var viewmodel = new InvoicesListViewModel()
            {
                Invoices = pagedlist,
                PageSize = vm.PageSize,
                Count = pageCount,
                CurrentPage = currentPage,
                IsEditAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };
            return View(viewmodel);
        }
    }
}
