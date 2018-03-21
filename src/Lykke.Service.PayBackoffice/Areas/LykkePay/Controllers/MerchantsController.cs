using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackOffice.Filters;
using Core.Users;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using BackOffice.Areas.LykkePay.Models;
using BackOffice.Controllers;
using BackOffice.Translates;
using Lykke.Service.PayAuth.Client;
using PagedList.Core;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuAssets)]
    public class MerchantsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayAuthClient _payAuthClient;
        private const string ErrorMessageAnchor = "#errorMessage";
        public MerchantsController(
            IPayInternalClient payInternalClient,
            IPayAuthClient payAuthClient)
        {
            _payInternalClient = payInternalClient;
            _payAuthClient = payAuthClient;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsPage()
        {
            var model = new MerchantsListViewModel();
            model.CurrentPage = 1;
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsList(MerchantsListViewModel vm)
        {
            var merchants = await _payInternalClient.GetMerchantsAsync();

            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);
            var list = new List<MerchantModel>(merchants).AsQueryable();
            if (!string.IsNullOrEmpty(vm.SearchValue))
                list = list.Where(x => x.Name.ToLower().Contains(vm.SearchValue.ToLower()) || x.ApiKey.ToLower().Contains(vm.SearchValue.ToLower())).AsQueryable();
            var pagedlist = new List<MerchantModel>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)list.Count() / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (list.Count() != 0)
                pagedlist = list.ToPagedList(currentPage, vm.PageSize).ToList();

            var viewmodel = new MerchantsListViewModel()
            {
                Merchants = pagedlist,
                PageSize = vm.PageSize,
                Count = pageCount,
                CurrentPage = currentPage
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchantDialog(string id = null)
        {
            var merchant = new MerchantModel();
            if (id != null)
            {
                merchant = await _payInternalClient.GetMerchantByIdAsync(id);
            }
            var viewModel = new AddOrEditMerchantDialogViewModel
            {
                Caption = "Add merchant",
                IsNewMerchant = id == null,
                ApiKey = merchant.ApiKey,
                DeltaSpread = merchant.DeltaSpread,
                Id = id,
                LpMarkupPercent = merchant.LpMarkupPercent,
                LpMarkupPips = merchant.LpMarkupPips,
                LwId = merchant.LwId,
                MarkupFixedFee = merchant.MarkupFixedFee,
                Name = merchant.Name,
                PublicKey = merchant.PublicKey,
                TimeCacheRates = merchant.TimeCacheRates,
                Certificate = merchant.PublicKey,
                SystemId = string.Empty
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchant(AddOrEditMerchantDialogViewModel vm)
        {
            var merchants = await _payInternalClient.GetMerchantsAsync();
            if (string.IsNullOrEmpty(vm.ApiKey))
                return this.JsonFailResult("ApiKey id required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.Name))
                return this.JsonFailResult("Name required", ErrorMessageAnchor);

            if (vm.IsNewMerchant)
            {
                if (string.IsNullOrEmpty(vm.SystemId))
                    return this.JsonFailResult("System id required", ErrorMessageAnchor);
                if (string.IsNullOrEmpty(vm.PublicKey))
                    return this.JsonFailResult("Public key required", ErrorMessageAnchor);
                if (merchants != null && merchants.Select(x => x.Name).Contains(vm.Name))
                {
                    return this.JsonFailResult(Phrases.AlreadyExists, "#name");
                }
                var merchant = await _payInternalClient.CreateMerchantAsync(new CreateMerchantRequest
                {
                    Name = vm.Name,
                    ApiKey = vm.ApiKey,
                    DeltaSpread = vm.DeltaSpread,
                    LpMarkupPercent = vm.LpMarkupPercent,
                    LpMarkupPips = vm.LpMarkupPips,
                    LwId = vm.LwId,
                    MarkupFixedFee = vm.MarkupFixedFee,
                    TimeCacheRates = vm.TimeCacheRates,
                });

                await _payAuthClient.RegisterAsync(new Lykke.Service.PayAuth.Client.Models.RegisterRequest
                {
                    ApiKey = vm.ApiKey,
                    Certificate = vm.PublicKey,
                    ClientId = merchant.Id,
                    SystemId = vm.SystemId
                });
            }
            else
            {
                var updatereq = new UpdateMerchantRequest
                {
                    Id = vm.Id,
                    ApiKey = vm.ApiKey,
                    DeltaSpread = vm.DeltaSpread,
                    LpMarkupPercent = vm.LpMarkupPercent,
                    LpMarkupPips = vm.LpMarkupPips,
                    LwId = vm.LwId,
                    MarkupFixedFee = vm.MarkupFixedFee,
                    TimeCacheRates = vm.TimeCacheRates,
                    Name = vm.Name
                };

                await _payInternalClient.UpdateMerchantAsync(updatereq);
            }

            return this.JsonRequestResult("#MerchantsPage", Url.Action("MerchantsList"));
        }
        [HttpPost]
        public ActionResult DeleteMerchantDialog(string merchant, string id)
        {
            var viewModel = new DeleteMerchantDialogViewModel
            {
                Caption = "Delete merchant",
                Name = merchant,
                Id = id
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMerchant(DeleteMerchantDialogViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.Id))
            {
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmDeleteMerchant");
            }
            await _payInternalClient.DeleteMerchantAsync(vm.Id);

            return this.JsonRequestResult("#MerchantsPage", Url.Action("MerchantsList"));
        }
    }
}
