using BackOffice.Areas.LykkePay.Models;
using BackOffice.Binders;
using BackOffice.Controllers;
using BackOffice.Helpers;
using BackOffice.Translates;
using Lykke.Service.BackofficeMembership.Client.Filters;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayPaymentRequests)]
    public class PaymentRequestsController : Controller
    {
        protected string BlockchainExplorerUrl
               => AzureBinder.BlockchainExplorerUrl;

        protected string EthereumBlockchainExplorerUrl
            => AzureBinder.EthereumBlockchainExplorerUrl;

        private readonly IPayInternalClient _payInternalClient;
        private readonly IMapper _mapper;

        public PaymentRequestsController(IPayInternalClient payInternalClient,
            IMapper mapper)
        {
            _payInternalClient = payInternalClient;
            _mapper = mapper;
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
                Merchants = merchants,
                Statuses = Enum.GetValues(typeof(PaymentRequestStatusHelper)).Cast<PaymentRequestStatusHelper>().ToList()
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
            else
                requests = await _payInternalClient.GetPaymentRequestsAsync(vm.SelectedMerchant);

            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);

            var list = new List<PaymentRequestModel>(requests).AsQueryable();
            try
            {
                if (!string.IsNullOrEmpty(vm.SearchValue))
                {
                    list = list.Where(x =>
                        (!string.IsNullOrEmpty(x.WalletAddress) && x.WalletAddress.Contains(vm.SearchValue))
                        || x.Id.Contains(vm.SearchValue)
                        || (!string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(vm.SearchValue))).AsQueryable();
                }

                if (vm.SelectedStatus != PaymentRequestStatusHelper.None)
                    list = list.Where(x => x.Status.ToString() == vm.SelectedStatus.ToString()).AsQueryable();
            }
            catch (Exception ex)
            {
                list = new List<PaymentRequestModel>().AsQueryable();
            }

            var pagedlist = new List<PaymentRequestModel>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)list.Count() / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (list.Count() != 0)
                pagedlist = list.OrderByDescending(x=>x.DueDate).ToPagedList(currentPage, vm.PageSize).ToList();

            var requestViewModels = _mapper.Map<PaymentRequestViewModel[]>(pagedlist);
            if (requestViewModels.Any())
            {
                var assetsList = (await _payInternalClient.GetAssetGeneralSettingsAsync()).ToArray();
                foreach (var requestViewModel in requestViewModels)
                {
                    requestViewModel.PaymentAssetGeneralSettings = assetsList.FirstOrDefault(a =>
                        string.Equals(a.AssetDisplayId, requestViewModel.PaymentAssetId,
                            StringComparison.OrdinalIgnoreCase));

                    requestViewModel.SettlementAssetGeneralSettings = assetsList.FirstOrDefault(a =>
                        string.Equals(a.AssetDisplayId, requestViewModel.SettlementAssetId,
                            StringComparison.OrdinalIgnoreCase));
                }
            }

            var viewModel = new PaymentRequestListViewModel()
            {
                Requests = requestViewModels,
                SelectedMerchant = vm.SelectedMerchant,
                BlockchainExplorerUrl = BlockchainExplorerUrl + "address/",
                EthereumBlockchainExplorerUrl = EthereumBlockchainExplorerUrl + "/address/",
                PageSize = vm.PageSize,
                Count = pageCount,
                CurrentPage = currentPage
            };

            return View(viewModel);
        }
    }
}
