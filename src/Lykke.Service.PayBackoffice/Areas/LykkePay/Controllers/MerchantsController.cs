using BackOffice.Areas.LykkePay.Models;
using BackOffice.Areas.LykkePay.Models.Merchants;
using BackOffice.Controllers;
using BackOffice.Helpers;
using BackOffice.Translates;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.BackofficeMembership.Client.Filters;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayMerchant.Client.Models;

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
        private readonly IPayMerchantClient _payMerchantClient;
        private const string ErrorMessageAnchor = "#errorMessage";

        public MerchantsController(
            IPayInternalClient payInternalClient,
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient, 
            IPayMerchantClient payMerchantClient)
        {
            _payInternalClient = payInternalClient;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _payMerchantClient = payMerchantClient;
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
            model.IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsList(MerchantsListViewModel vm)
        {
            var merchants = await _payMerchantClient.Api.GetAllAsync();
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
                catch (Exception)
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
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchantDialog(string id = null)
        {
            var merchant = new MerchantModel();
            if (id != null)
            {
                merchant = await _payMerchantClient.Api.GetByIdAsync(id);
            }
            

            var viewModel = new AddOrEditMerchantDialogViewModel
            {
                Caption = "Add merchant",
                IsNewMerchant = id == null,
                ApiKey = merchant.ApiKey,
                Id = id,
                LwId = merchant.LwId,
                Name = merchant.Name,
                DisplayName = merchant.DisplayName                
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchant(AddOrEditMerchantDialogViewModel vm)
        {
            var merchants = await _payMerchantClient.Api.GetAllAsync();
            if (string.IsNullOrEmpty(vm.ApiKey))
                return this.JsonFailResult("ApiKey id required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.Name))
                return this.JsonFailResult("Name required", ErrorMessageAnchor);
            if (string.IsNullOrEmpty(vm.DisplayName))
                return this.JsonFailResult("DisplayName required", ErrorMessageAnchor);

            if (vm.IsNewMerchant)
            {
                if (merchants != null && (merchants.Select(x => x.Name).Contains(vm.Name) || merchants.Select(x => x.ApiKey).Contains(vm.ApiKey)))
                {
                    return this.JsonFailResult(Phrases.AlreadyExists, "#name");
                }
                try
                {
                    var merchant = await _payMerchantClient.Api.CreateAsync(new CreateMerchantRequest
                    {
                        Name = vm.Name,
                        ApiKey = vm.ApiKey,
                        LwId = vm.LwId,
                        DisplayName = vm.Name
                    });

                    await _payAuthClient.RegisterAsync(new Lykke.Service.PayAuth.Client.Models.RegisterRequest
                    {
                        ApiKey = vm.ApiKey,
                        ClientId = merchant.Id
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
                    LwId = vm.LwId,
                    Name = vm.Name,
                    DisplayName = vm.DisplayName
                };

                await _payMerchantClient.Api.UpdateAsync(updatereq);
            }
            return this.JsonRequestResult("#merchantsList", Url.Action("MerchantsList"));
        }
        [HttpPost]
        public async Task<ActionResult> UploadLogoDialog(string id = null)
        {
            var merchantfiles = (await _payInternalClient.GetFilesAsync(id)).ToList();
            byte[] merchantlogo = null;
            if (merchantfiles.Any())
                merchantlogo = await _payInternalClient.GetFileAsync(id, merchantfiles[0].Id);

            var viewModel = new UploadLogoDialogViewModel
            {
                Caption = "Merchant logo",
                MerchantId = id,
                LogoImage = merchantlogo == null ? string.Empty : Convert.ToBase64String(merchantlogo)
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<ActionResult> UploadLogo(IFormFile file)
        {
            const int maxFileSize = 10 * 1024 * 1024;
            var merchantId = Request.Form["MerchantId"];

            var merchantfiles = (await _payInternalClient.GetFilesAsync(merchantId)).ToList();
            foreach (var fileitem in merchantfiles)
            {
                await _payInternalClient.DeleteFileAsync(merchantId, fileitem.Id);
            }

            if (file == null)
                file = Request.Form.Files.FirstOrDefault();

            if (file != null && file.Length <= maxFileSize)
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);

                        if (ms.Length > 0)
                        {
                            string contentType = file.ContentType;
                            byte[] imageBytes = ms.ToArray();
                            await _payInternalClient.UploadFileAsync(merchantId, imageBytes, file.FileName, contentType);
                        }
                    }
                }
            }

            return this.JsonRequestResult("#merchantsList", Url.Action("MerchantsList", new { id = merchantId }));
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
            await _payMerchantClient.Api.DeleteAsync(vm.Id);

            return this.JsonRequestResult("#merchantsList", Url.Action("MerchantsList"));
        }

        [HttpPost]
        public async Task<ActionResult> MerchantsSettingsPage()
        {
            var model = new MerchantSettingsListViewModel();
            model.Merchants = await _payMerchantClient.Api.GetAllAsync();
            model.CurrentPage = 1;
            model.IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull);
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> MerchantsSettingsList(MerchantSettingsListViewModel vm)
        {
            MerchantSetting setting;

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
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
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
            try
            {
                await _payInvoiceClient.SetMerchantSettingAsync(setting);
            }
            catch(Lykke.Service.PayInvoice.Client.ErrorResponseException ex)
            {
                return this.JsonFailResult(ex.Error.ErrorMessage, ErrorMessageAnchor);
            }
            return this.JsonRequestResult("#merchantsSettingsList", Url.Action("MerchantsSettingsList"), new MerchantSettingsListViewModel() { SelectedMerchant = vm.MerchantId });
        }
    }
}
