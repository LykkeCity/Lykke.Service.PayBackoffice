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

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuAssets)]
    public class AssetsController : Controller
    {
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
        public async Task<ActionResult> AssetPaymentList()
        {
            var assetPaymentList = await _payInternalClient.GetAvailableAsync(AssetAvailabilityType.Payment);
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
            var assetSettlementList = await _payInternalClient.GetAvailableAsync(AssetAvailabilityType.Settlement);
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
        public async Task<ActionResult> AddAssetPayment(AssetModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = true;
            request.AvailabilityType = AssetAvailabilityType.Payment;
            await _payInternalClient.SetAvailabilityAsync(request);
            return this.JsonRequestResult("#assetPaymentList", Url.Action("AssetPaymentList"));
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetPayment(AddAssetPaymentDialogViewModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = false;
            request.AvailabilityType = AssetAvailabilityType.Payment;
            await _payInternalClient.SetAvailabilityAsync(request);
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
        public async Task<ActionResult> AddAssetSettlement(AssetModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = true;
            request.AvailabilityType = AssetAvailabilityType.Settlement;
            await _payInternalClient.SetAvailabilityAsync(request);
            return this.JsonRequestResult("#assetSettlementList", Url.Action("AssetSettlementList"));
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAssetSettlement(AddAssetPaymentDialogViewModel model)
        {
            var request = new UpdateAssetAvailabilityRequest();
            request.AssetId = model.Id;
            request.Value = false;
            request.AvailabilityType = AssetAvailabilityType.Settlement;
            await _payInternalClient.SetAvailabilityAsync(request);
            return this.JsonRequestResult("#assetSettlementList", Url.Action("AssetSettlementList"));
        }
    }
}
