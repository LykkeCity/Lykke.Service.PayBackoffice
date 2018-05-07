using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Users;
using BackOffice.Filters;
using Lykke.Service.PayInternal.Client;
using System.Threading.Tasks;
using BackOffice.Areas.LykkePay.Models;
using BackOffice.Controllers;
using System;
using Lykke.Service.PayInternal.Client.Models.Markup;
using System.Collections.Generic;
using System.Linq;
using PagedList.Core;
using BackOffice.Translates;

namespace Lykke.Service.PayBackoffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayMerchantsView)]
    public class MarkupsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        public MarkupsController(
            IPayInternalClient payInternalClient)
        {
            _payInternalClient = payInternalClient;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<ActionResult> MarkupsPage()
        {
            var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();
            var model = new MarkupsListViewModel();
            model.Merchants = merchants;
            model.CurrentPage = 1;
            model.IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
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
                IsEditAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
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
                Pips = markup.Pips
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMarkup(AddOrEditMarkupDialogViewModel vm)
        {
            var request = new UpdateMarkupRequest();
            request.DeltaSpread = vm.DeltaSpread;
            request.FixedFee = vm.FixedFee;
            request.Percent = vm.Percent;
            request.Pips = vm.Pips;
            if (string.IsNullOrEmpty(vm.SelectedMerchant) || vm.SelectedMerchant == "Default")
                await _payInternalClient.SetDefaultMarkupAsync(vm.AssetPairId, request);
            else
                await _payInternalClient.SetMarkupForMerchantAsync(vm.SelectedMerchant, vm.AssetPairId, request);
            return this.JsonRequestResult("#markupsList", Url.Action("MarkupsList"), new StaffsPageViewModel() { SelectedMerchant = vm.SelectedMerchant });
        }
    }
}
