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
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Core.Domain;
using System.Net;
using Lykke.Service.PayInternal.Client.Exceptions;
using BackOffice.Areas.LykkePay.Models.Merchants;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.LykkePayMerchantsView)]
    public class MerchantsController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private const string ErrorMessageAnchor = "#errorMessage";
        public MerchantsController(
            IPayInternalClient payInternalClient,
            IPayAuthClient payAuthClient, IPayInvoiceClient payInvoiceClient)
        {
            _payInternalClient = payInternalClient;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
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
            model.IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
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
            if (!string.IsNullOrEmpty(vm.SearchValue) && !vm.FilterByEmail)
                list = list.Where(x => x.Name.ToLower().Contains(vm.SearchValue.ToLower()) || x.ApiKey.ToLower().Contains(vm.SearchValue.ToLower())).AsQueryable();
            if (vm.FilterByEmail)
            {
                try
                {
                    var allstaffs = await _payInvoiceClient.GetEmployeesAsync();
                    var filteredstaffs = allstaffs.Where(s => !string.IsNullOrEmpty(s.Email) && s.Email.Contains(vm.SearchValue)).GroupBy(x => x.MerchantId).ToList();
                    var filtered = new List<MerchantModel>();
                    foreach (var merchant in filteredstaffs)
                    {
                        var model = merchants.FirstOrDefault(m => m.Id == merchant.Key);
                        if (model != null)
                            filtered.Add(model);
                    }
                    list = filtered.AsQueryable();
                }
                catch (Exception ex)
                {
                    list = new List<MerchantModel>().AsQueryable();
                }
            }
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
                CurrentPage = currentPage,
                IsEditAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
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
                SystemId = string.Empty,
                DisplayName = merchant.DisplayName
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
            if (string.IsNullOrEmpty(vm.DisplayName))
                return this.JsonFailResult("DisplayName required", ErrorMessageAnchor);

            if (vm.IsNewMerchant)
            {
                if (string.IsNullOrEmpty(vm.SystemId))
                    return this.JsonFailResult("System id required", ErrorMessageAnchor);
                if (string.IsNullOrEmpty(vm.PublicKey))
                    return this.JsonFailResult("Public key required", ErrorMessageAnchor);
                if (merchants != null && (merchants.Select(x => x.Name).Contains(vm.Name) || merchants.Select(x => x.ApiKey).Contains(vm.ApiKey)))
                {
                    return this.JsonFailResult(Phrases.AlreadyExists, "#name");
                }
                try
                {
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
                        DisplayName = vm.Name
                    });

                    await _payAuthClient.RegisterAsync(new Lykke.Service.PayAuth.Client.Models.RegisterRequest
                    {
                        ApiKey = vm.ApiKey,
                        Certificate = vm.PublicKey,
                        ClientId = merchant.Id,
                        SystemId = vm.SystemId
                    });
                }
                catch (Exception ex)
                {
                    return this.JsonFailResult(ex.Message, ErrorMessageAnchor);
                }
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
                    Name = vm.Name,
                    DisplayName = vm.DisplayName
                };

                await _payInternalClient.UpdateMerchantAsync(updatereq);
            }

            return this.JsonRequestResult("#merchantsList", Url.Action("MerchantsList"));
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

            return this.JsonRequestResult("#merchantsList", Url.Action("MerchantsList"));
        }

        [HttpPost]
        public async Task<ActionResult> MerchantsSettingsPage()
        {
            var model = new MerchantSettingsListViewModel();
            model.Merchants = await _payInternalClient.GetMerchantsAsync();
            model.CurrentPage = 1;
            model.IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsSettingsList(MerchantSettingsListViewModel vm)
        {
            var setting = new MerchantSetting();
            try
            {
                setting = await _payInvoiceClient.GetMerchantSettingAsync(vm.SelectedMerchant);
            }
            catch(Lykke.Service.PayInvoice.Client.ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                setting = null;
            }
            vm.PageSize = vm.PageSize == 0 ? 10 : vm.PageSize;
            var pagesize = Request.Cookies["PageSize"];
            if (pagesize != null)
                vm.PageSize = Convert.ToInt32(pagesize);
            var list = new List<MerchantSetting>();
            if (setting != null)
                list.Add(setting);
            var pagedlist = new List<MerchantSetting>();
            var pageCount = Convert.ToInt32(Math.Ceiling((double)list.Count() / vm.PageSize));
            var currentPage = vm.CurrentPage == 0 ? 1 : vm.CurrentPage;
            if (list.Count() != 0)
                pagedlist = list.AsQueryable().ToPagedList(currentPage, vm.PageSize).ToList();
            var viewmodel = new MerchantSettingsListViewModel()
            {
                Settings = pagedlist,
                PageSize = vm.PageSize,
                Count = pageCount,
                CurrentPage = currentPage,
                IsEditAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (await this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchantSettingDialog(AddOrEditMerchantSettingDialog vm)
        {
            vm.Caption = string.IsNullOrEmpty(vm.BaseAsset) ? "Add setting" : "Edit setting";
            return View(vm);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchantSetting(AddOrEditMerchantSettingDialog vm)
        {
            if (string.IsNullOrEmpty(vm.BaseAsset))
                return this.JsonFailResult("BaseAsset required", ErrorMessageAnchor);
            var setting = new MerchantSetting();
            setting.MerchantId = vm.MerchantId;
            setting.BaseAsset = vm.BaseAsset;
            await _payInvoiceClient.SetMerchantSettingAsync(setting);
            return this.JsonRequestResult("#merchantsSettingsList", Url.Action("MerchantsSettingsList"), new MerchantSettingsListViewModel() { SelectedMerchant = vm.MerchantId });
        }
    }
}
