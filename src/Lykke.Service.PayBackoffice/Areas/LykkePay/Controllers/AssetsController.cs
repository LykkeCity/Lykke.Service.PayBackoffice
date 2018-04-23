using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackOffice.Filters;
using Core.Users;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Asset;
using BackOffice.Controllers;
using BackOffice.Areas.LykkePay.Models;
using BackOffice.Translates;
using AutoMapper;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuAssets)]
    public class AssetsController : Controller
    {
        private const string ErrorMessageAnchor = "#errorMessage";
        private readonly IPayInternalClient _payInternalClient;
        public AssetsController(
            IPayInternalClient payInternalClient)
        {
            _payInternalClient = payInternalClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AssetPayment()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AssetByMerchant(string merchant = "")
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
            return View(new AssetByMerchantViewModel
            {
                SelectedMerchant = merchant,
                Merchants = merchants
            });
        }
        [HttpPost]
        public async Task<ActionResult> AssetByMerchantList(AssetByMerchantViewModel vm)
        {
            var assets = await _payInternalClient.GetPersonalAvailableAssetsAsync(vm.SelectedMerchant);
            return View(assets);
        }
        [HttpPost]
        public async Task<ActionResult> AssetPaymentList()
        {
            var assetPaymentList = await _payInternalClient.GetGeneralAvailableAssetsAsync(AssetAvailabilityType.Payment);
            return View(assetPaymentList.Assets);
        }
        [HttpPost]
        public async Task<ActionResult> AssetSettlement()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AssetSettlementList()
        {
            var assetSettlementList = await _payInternalClient.GetGeneralAvailableAssetsAsync(AssetAvailabilityType.Settlement);
            return View(assetSettlementList.Assets);
        }
        [HttpPost]
        public async Task<ActionResult> AddAssetPaymentDialog()
        {
            var vm = new AddAssetPaymentDialogViewModel
            {
                Caption = "Add payment asset",
                Id = string.Empty
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetPaymentDialog(string assetId = "")
        {
            var vm = new AddAssetPaymentDialogViewModel
            {
                Caption = "Delete payment asset",
                Id = assetId
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> AddAssetByMerchantDialog(string merchant = null)
        {
            var assets = await _payInternalClient.GetPersonalAvailableAssetsAsync(merchant);
            var vm = new AddAssetsByMerchantDialogViewModel
            {
                Caption = "Add assets to merchant",
                MerchantId = merchant,
                PaymentAssets = assets?.PaymentAssets ?? string.Empty,
                SettlementAssets = assets?.SettlementAssets ?? string.Empty,
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> AddAssetByMerchant(AddAssetsByMerchantDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.PaymentAssets))
                return this.JsonFailResult("PaymentAssets required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.SettlementAssets))
                return this.JsonFailResult("SettlementAssets required", ErrorMessageAnchor);
            vm.SettlementAssets = vm.SettlementAssets.Replace(' ', ';').Replace(',', ';');
            vm.PaymentAssets = vm.PaymentAssets.Replace(' ', ';').Replace(',', ';');
            await _payInternalClient.SetPersonalAvailableAssetsAsync(new UpdateAssetAvailabilityByMerchantRequest()
            {
                MerchantId = vm.MerchantId,
                PaymentAssets = vm.PaymentAssets,
                SettlementAssets = vm.SettlementAssets
            });
            return this.JsonRequestResult("#assetByMerchantList", Url.Action("AssetByMerchantList"), new AssetByMerchantViewModel() { SelectedMerchant = vm.MerchantId });
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetByMerchantDialog(string merchantId = "")
        {
            var viewModel = new AddAssetsByMerchantDialogViewModel()
            {
                Caption = "Delete assets from merchant",
                MerchantId = merchantId
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAssetByMerchant(AddAssetsByMerchantDialogViewModel vm)
        {
            await _payInternalClient.SetPersonalAvailableAssetsAsync(new UpdateAssetAvailabilityByMerchantRequest()
            {
                MerchantId = vm.MerchantId
            });
            return this.JsonRequestResult("#assetByMerchantList", Url.Action("AssetByMerchantList"), new AssetByMerchantViewModel() { SelectedMerchant = vm.MerchantId });
        }
        [HttpPost]
        public async Task<ActionResult> AddAssetPayment(AddAssetPaymentDialogViewModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = true;
            request.AvailabilityType = AssetAvailabilityType.Payment;
            await _payInternalClient.SetGeneralAvailableAssetsAsync(request);
            return this.JsonRequestResult("#assetPaymentList", Url.Action("AssetPaymentList"));
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetPayment(AddAssetPaymentDialogViewModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = false;
            request.AvailabilityType = AssetAvailabilityType.Payment;
            await _payInternalClient.SetGeneralAvailableAssetsAsync(request);
            return this.JsonRequestResult("#assetPaymentList", Url.Action("AssetPaymentList"));
        }
        [HttpPost]
        public async Task<ActionResult> AddAssetSettlementDialog()
        {
            var vm = new AddAssetPaymentDialogViewModel
            {
                Caption = "Add settlement asset",
                Id = string.Empty
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetSettlementDialog(string assetId = "")
        {
            var vm = new AddAssetPaymentDialogViewModel
            {
                Caption = "Delete settlement asset",
                Id = assetId
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> AddAssetSettlement(AddAssetPaymentDialogViewModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = true;
            request.AvailabilityType = AssetAvailabilityType.Settlement;
            await _payInternalClient.SetGeneralAvailableAssetsAsync(request);
            return this.JsonRequestResult("#assetSettlementList", Url.Action("AssetSettlementList"));
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetSettlement(AddAssetPaymentDialogViewModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = false;
            request.AvailabilityType = AssetAvailabilityType.Settlement;
            await _payInternalClient.SetGeneralAvailableAssetsAsync(request);
            return this.JsonRequestResult("#assetSettlementList", Url.Action("AssetSettlementList"));
        }
    }
}
