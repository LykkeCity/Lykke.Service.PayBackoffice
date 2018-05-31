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
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Common.Api.Contract.Responses;
using BackOffice.Areas.LykkePay.Models.Assets;
using Lykke.Service.PayInternal.Client.Models;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayMerchantsView)]
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
        public async Task<ActionResult> GeneralSettings()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> GeneralSettingsList()
        {
            var assetsList = await _payInternalClient.GetAssetGeneralSettingsAsync();
            return View(assetsList);
        }
        [HttpPost]
        public async Task<ActionResult> AddGeneralSettingsDialog()
        {
            var vm = new AddGeneralSettingsDialogViewModel
            {
                Caption = "Add general setting",
                AssetDisplayId = string.Empty,
                NetworkList = Enum.GetNames(typeof(BlockchainType)).ToList()
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteGeneralSettingsDialog(string assetId = "")
        {
            var vm = new AddGeneralSettingsDialogViewModel
            {
                Caption = "Delete payment asset",
                AssetDisplayId = assetId
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> AddGeneralSettings(AddGeneralSettingsDialogViewModel model)
        {
            if (string.IsNullOrEmpty(model.AssetDisplayId))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, ErrorMessageAnchor);

            var request = new UpdateAssetGeneralSettingsRequest();
            request.AssetDisplayId = model.AssetDisplayId;
            request.Network = (BlockchainType)Enum.Parse(typeof(BlockchainType), model.SelectedNetwork);
            request.PaymentAvailable = model.PaymentAvailable;
            request.SettlementAvailable = model.SettlementAvailable;
            try
            {
                await _payInternalClient.SetAssetGeneralSettingsAsync(request);
            }
            catch (DefaultErrorResponseException ex)
            {
                if (ex.InnerException != null)
                {
                    var content = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorResponse>(((Refit.ApiException)ex.InnerException).Content);
                    return this.JsonFailResult(content.ErrorMessage, ErrorMessageAnchor);
                }
                else
                    return this.JsonFailResult(ex.Message, ErrorMessageAnchor);
            }
            return this.JsonRequestResult("#generalSettingsList", Url.Action("GeneralSettingsList"));
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsSettings(string merchant = "")
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
            return View(new MerchantsSettingsViewModel
            {
                SelectedMerchant = merchant,
                Merchants = merchants
            });
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsSettingsList(MerchantsSettingsViewModel vm)
        {
            var assets = await _payInternalClient.GetAssetMerchantSettingsAsync(vm.SelectedMerchant);
            return View(assets);
        }

        [HttpPost]
        public async Task<ActionResult> AddMerchantSettingDialog(string merchant = null)
        {
            var assets = await _payInternalClient.GetAssetMerchantSettingsAsync(merchant);
            var vm = new AddMerchantSettingDialogViewModel
            {
                Caption = "Add assets to merchant",
                MerchantId = merchant,
                PaymentAssets = assets?.PaymentAssets ?? string.Empty,
                SettlementAssets = assets?.SettlementAssets ?? string.Empty,
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> AddMerchantSetting(AddMerchantSettingDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.PaymentAssets))
                return this.JsonFailResult("PaymentAssets required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.SettlementAssets))
                return this.JsonFailResult("SettlementAssets required", ErrorMessageAnchor);
            vm.SettlementAssets = vm.SettlementAssets.Replace(' ', ';').Replace(',', ';');
            vm.PaymentAssets = vm.PaymentAssets.Replace(' ', ';').Replace(',', ';');
            vm.SettlementAssets = String.Join(";", vm.SettlementAssets.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToArray());
            vm.PaymentAssets = String.Join(";", vm.PaymentAssets.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToArray());
            await _payInternalClient.SetAssetMerchantSettingsAsync(new UpdateAssetMerchantSettingsRequest()
            {
                MerchantId = vm.MerchantId,
                PaymentAssets = vm.PaymentAssets,
                SettlementAssets = vm.SettlementAssets
            });
            return this.JsonRequestResult("#merchantsSettingsList", Url.Action("MerchantsSettingsList"), new MerchantsSettingsViewModel() { SelectedMerchant = vm.MerchantId });
        }
    }
}
