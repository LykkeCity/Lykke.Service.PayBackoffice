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
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

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
        public IActionResult Index(string merchantId)
        {
            var model = new InvoicesListViewModel();
            model.SelectedMerchant = merchantId;
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> InvoicesPage(InvoicesListViewModel vm)
        {
            vm.Merchants = await _payInternalClient.GetMerchantsAsync();
            vm.CurrentPage = 1;
            vm.IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> InvoicesList(InvoicesListViewModel vm)
        {
            var invoices = await _payInvoiceClient.GetMerchantInvoicesAsync(vm.SelectedMerchant);
            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);
            
            
            var pagedlist = new List<InvoiceModel>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)invoices.Count() / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (invoices.Count() != 0)
                pagedlist = invoices.AsQueryable().ToPagedList(currentPage, vm.PageSize).ToList();
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
        public IActionResult PaymentRequests(string invoiceId)
        {
            var model = new InvoicePaymentRequestsViewModel();
            model.InvoiceId = invoiceId;
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> PaymentRequestsPage(InvoicePaymentRequestsViewModel vm)
        {
            var invoice = await _payInvoiceClient.GetInvoiceAsync(vm.InvoiceId);
            vm.CurrentPage = 1;
            vm.MerchantId = invoice.MerchantId;
            vm.PaymentRequestId = invoice.PaymentRequestId;
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> PaymentRequestsList(InvoicePaymentRequestsViewModel vm)
        {
            var request = await _payInternalClient.GetPaymentRequestAsync(vm.MerchantId, vm.PaymentRequestId);
            var list = new List<PaymentRequestModel>();
            list.Add(request);
            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);


            var pagedlist = new List<PaymentRequestModel>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)list.Count() / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (list.Count() != 0)
                pagedlist = list.AsQueryable().ToPagedList(currentPage, vm.PageSize).ToList();
            var viewmodel = new InvoicePaymentRequestsViewModel()
            {
                Requests = pagedlist,
                PageSize = vm.PageSize,
                Count = pageCount,
                CurrentPage = currentPage
            };
            return View(viewmodel);
        }
    }
}
