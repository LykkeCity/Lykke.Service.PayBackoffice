using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackOffice.Areas.LykkePay.Models;
using BackOffice.Controllers;
using BackOffice.Filters;
using BackOffice.Translates;
using Core.Users;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuAssets)]
    public class PaymentRequestsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        public PaymentRequestsController(
            IPayInternalClient payInternalClient)
        {
            _payInternalClient = payInternalClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> PaymentRequestsPage(string merchant = "")
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

            return View(new PaymentRequestPageViewModel()
            {
                SelectedMerchant = merchant,
                Merchants = merchants
            });
        }
        [HttpPost]
        public async Task<ActionResult> PaymentRequestsList(PaymentRequestPageViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.SelectedMerchant))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#selectedMerchant");
            var merchant = new MerchantModel();
            if (!string.IsNullOrEmpty(vm.MerchantName))
            {
                var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();
                merchant = merchants.FirstOrDefault(m => String.Equals(m.Name, vm.MerchantName, StringComparison.CurrentCultureIgnoreCase));
            }
            IReadOnlyList<PaymentRequestModel> requests;
            if (!string.IsNullOrEmpty(merchant?.Id))
                requests = await _payInternalClient.GetPaymentRequestsAsync(merchant.Id);
            else requests = await _payInternalClient.GetPaymentRequestsAsync(vm.SelectedMerchant);

            var viewModel = new PaymentRequestListViewModel()
            {
                Requests = requests,
                SelectedMerchant = vm.SelectedMerchant
            };

            return View(viewModel);
        }
    }
}
