using BackOffice.Areas.LykkePay.Models;
using BackOffice.Controllers;
using BackOffice.Helpers;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.BackofficeMembership.Client.Filters;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayMerchant.Client;

namespace Lykke.Service.PayBackoffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayMerchantsView)]
    public class MarkupsController : Controller
    {
        private const string ErrorMessageAnchor = "#errorMessage";
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayMerchantClient _payMerchantClient;

        public MarkupsController(
            IPayInternalClient payInternalClient, 
            IPayMerchantClient payMerchantClient)
        {
            _payInternalClient = payInternalClient;
            _payMerchantClient = payMerchantClient;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<ActionResult> MarkupsPage()
        {
            var merchants = await _payMerchantClient.Api.GetAllAsync();
            var model = new MarkupsListViewModel();
            model.Merchants = merchants;
            model.CurrentPage = 1;
            model.IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> MarkupsList(MarkupsListViewModel vm)
        {
            var markups = new List<MarkupResponse>();
            if (string.IsNullOrEmpty(vm.SelectedMerchant) || vm.SelectedMerchant == "Default")
                markups = (await _payInternalClient.GetDefaultMarkupsAsync()).ToList();
            else
                markups = (await _payInternalClient.GetMarkupsForMerchantAsync(vm.SelectedMerchant)).ToList();
            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);
            var list = new List<MarkupResponse>(markups).AsQueryable();
            var pagedlist = new List<MarkupResponse>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)list.Count() / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (list.Count() != 0)
                pagedlist = list.ToPagedList(currentPage, vm.PageSize).ToList();
            var viewmodel = new MarkupsListViewModel()
            {
                Markups = pagedlist,
                PageSize = vm.PageSize,
                Count = pageCount,
                SelectedMerchant = vm.SelectedMerchant,
                CurrentPage = currentPage,
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMarkupDialog(AddOrEditMarkupDialogViewModel vm)
        {
            var markup = new MarkupResponse();
            if (!string.IsNullOrEmpty(vm.AssetPairId) && vm.SelectedMerchant == "Default")
                markup = await _payInternalClient.GetDefaultMarkupAsync(vm.AssetPairId);
            else if (!string.IsNullOrEmpty(vm.AssetPairId))
                markup = await _payInternalClient.GetMarkupForMerchantAsync(vm.SelectedMerchant, vm.AssetPairId);
            var viewmodel = new AddOrEditMarkupDialogViewModel()
            {
                SelectedMerchant = vm.SelectedMerchant,
                AssetPairId = vm.AssetPairId,
                DeltaSpread = markup.DeltaSpread,
                FixedFee = markup.FixedFee,
                Percent = markup.Percent,
                Pips = markup.Pips,
                PriceAssetPairId = markup.PriceAssetPairId,
                PriceMethod = Enum.GetValues(typeof(PriceMethod)).Cast<PriceMethod>().ToList(),
                SelectedPriceMethod = markup.PriceMethod.ToString(),
            IsEditMode = !string.IsNullOrEmpty(vm.AssetPairId)
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMarkup(AddOrEditMarkupDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.AssetPairId))
                return this.JsonFailResult("AssetPairId required", ErrorMessageAnchor);
            if (!vm.IsEditMode)
            {
                var markup = new MarkupResponse();
                try
                {
                    if (!string.IsNullOrEmpty(vm.AssetPairId) && vm.SelectedMerchant == "Default")
                        markup = await _payInternalClient.GetDefaultMarkupAsync(vm.AssetPairId);
                    else if (!string.IsNullOrEmpty(vm.AssetPairId))
                        markup = await _payInternalClient.GetMarkupForMerchantAsync(vm.SelectedMerchant, vm.AssetPairId);
                }
                catch(DefaultErrorResponseException ex)
                {
                    markup = null;
                }
                if (markup != null)
                    return this.JsonFailResult("Markup exist: " + markup.AssetPairId, ErrorMessageAnchor);
            }
            var request = new UpdateMarkupRequest();
            request.DeltaSpread = vm.DeltaSpread;
            request.FixedFee = vm.FixedFee;
            request.Percent = vm.Percent;
            request.Pips = vm.Pips;
            request.PriceAssetPairId = vm.PriceAssetPairId;
            var pricemethod = PriceMethod.None;
            Enum.TryParse<PriceMethod>(vm.SelectedPriceMethod, out pricemethod);
            request.PriceMethod = pricemethod;
            try
            {
                if (string.IsNullOrEmpty(vm.SelectedMerchant) || vm.SelectedMerchant == "Default")
                    await _payInternalClient.SetDefaultMarkupAsync(vm.AssetPairId, request);
                else
                    await _payInternalClient.SetMarkupForMerchantAsync(vm.SelectedMerchant, vm.AssetPairId, request);
                return this.JsonRequestResult("#markupsList", Url.Action("MarkupsList"), new StaffsPageViewModel() { SelectedMerchant = vm.SelectedMerchant });
            }
            catch(DefaultErrorResponseException ex)
            {
                return this.JsonFailResult("Error: " + ex.Error.ErrorMessage, ErrorMessageAnchor);
            }
        }
    }
}
