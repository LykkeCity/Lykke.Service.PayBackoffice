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
using Common;
using System.IO.Compression;
using Lykke.Common.Log;
using Lykke.Service.PayAuth.Client.Models.GenerateRsaKeys;
using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.PayTransferValidation.Client;
using Lykke.Service.PayTransferValidation.Client.Models.MerchantConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        private readonly IPayTransferValidationClient _payTransferValidationClient;
        private readonly ILog _log;
        private const string ErrorMessageAnchor = "#errorMessage";

        public MerchantsController(
            IPayInternalClient payInternalClient,
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            IPayMerchantClient payMerchantClient,
            ILogFactory logFactory, 
            IPayTransferValidationClient payTransferValidationClient)
        {
            _payInternalClient = payInternalClient;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _payMerchantClient = payMerchantClient;
            _payTransferValidationClient = payTransferValidationClient;
            _log = logFactory.CreateLog(this);
        }

        #region Merchants

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MerchantsPage()
        {
            var model = new MerchantsListViewModel
            {
                CurrentPage = 1,
                IsFullAccess = this.GetUserRolesPair().HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };

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
                list = list.Where(x =>
                    x.Name.ToLower().Contains(vm.SearchValue.ToLower()) ||
                    x.ApiKey.ToLower().Contains(vm.SearchValue.ToLower())).AsQueryable();
            if (vm.FilterByEmail)
            {
                try
                {
                    var allstaffs = await _payInvoiceClient.GetEmployeesAsync();
                    var filteredstaffs = allstaffs
                        .Where(s => !string.IsNullOrEmpty(s.Email) && s.Email.Contains(vm.SearchValue))
                        .GroupBy(x => x.MerchantId).ToList();
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
            var pageCount = Convert.ToInt32(Math.Ceiling((double) list.Count() / vm.PageSize));
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

            bool isNewMerchant = id == null;

            if (!isNewMerchant)
            {
                merchant = await _payMerchantClient.Api.GetByIdAsync(id);
            }

            var viewModel = new AddOrEditMerchantDialogViewModel
            {
                Caption = isNewMerchant ? "Add merchant" : "Edit merchant",
                IsNewMerchant = id == null,
                ApiKey = merchant.ApiKey,
                Id = id,
                LwId = merchant.LwId,
                Name = merchant.Name,
                DisplayName = merchant.DisplayName,
                Email = merchant.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchant(AddOrEditMerchantDialogViewModel vm)
        {
            var merchants = await _payMerchantClient.Api.GetAllAsync();

            if (string.IsNullOrEmpty(vm.Name))
                return this.JsonFailResult("MerchantId required", ErrorMessageAnchor);

            if (string.IsNullOrEmpty(vm.DisplayName))
                return this.JsonFailResult("DisplayName required", ErrorMessageAnchor);

            if (string.IsNullOrEmpty(vm.ApiKey))
                vm.ApiKey = StringUtils.GenerateId().Replace("-", string.Empty);

            if (string.IsNullOrEmpty(vm.Email) && vm.IsNewMerchant)
                return this.JsonFailResult("Email required", ErrorMessageAnchor);

            if (vm.IsNewMerchant)
            {
                if (merchants != null && merchants.Select(x => x.Name).Contains(vm.Name))
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
                        DisplayName = vm.DisplayName,
                        Email = vm.Email
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
                MerchantModel existingMerchant = await _payMerchantClient.Api.GetByIdAsync(vm.Id);

                try
                {
                    var updateRequest = new UpdateMerchantRequest
                    {
                        Id = vm.Id,
                        ApiKey = vm.ApiKey,
                        LwId = vm.LwId,
                        Name = vm.Name,
                        DisplayName = vm.DisplayName,
                    };

                    if (string.IsNullOrEmpty(existingMerchant.Email))
                    {
                        if (!string.IsNullOrEmpty(vm.Email))
                            updateRequest.Email = vm.Email;
                    }

                    await _payMerchantClient.Api.UpdateAsync(updateRequest);

                    await _payAuthClient.UpdateApiKeyAsync(new Lykke.Service.PayAuth.Client.Models.UpdateApiKeyRequest
                    {
                        ApiKey = vm.ApiKey,
                        ClientId = vm.Id
                    });
                }
                catch (Exception ex)
                {
                    return this.JsonFailResult(ex.Message, ErrorMessageAnchor);
                }
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
                            await _payInternalClient.UploadFileAsync(merchantId, imageBytes, file.FileName,
                                contentType);
                        }
                    }
                }
            }

            return this.JsonRequestResult("#merchantsList", Url.Action("MerchantsList", new {id = merchantId}));
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

        #endregion

        #region Volatility settings

        [HttpPost]
        public IActionResult DeleteMerchantVolatilitySettingsDialog(string merchantId)
        {
            var viewModel = new DeleteMerchantVolatilitySettingsDialogViewModel
            {
                Caption = "Delete volatility settings for",
                MerchantId = merchantId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMerchantVolatilitySettings(
            DeleteMerchantVolatilitySettingsDialogViewModel vm)
        {
            try
            {
                await _payMerchantClient.Settings.DeleteVolatilitySettingsAsync(vm.MerchantId);
            }
            catch (ClientApiException e)
            {
                return this.JsonFailResult(e.Message, ErrorMessageAnchor);
            }

            return this.JsonRequestResult("#merchantVolatilitySettingsList",
                Url.Action("MerchantVolatilitySettingsList", new {SelectedMerchant = vm.MerchantId}));
        }

        [HttpPost]
        public async Task<IActionResult> MerchantVolatilitySettingsPage()
        {
            var model = new MerchantVolatilitySettingsListViewModel
            {
                Merchants = await _payMerchantClient.Api.GetAllAsync(),
                CurrentPage = 1,
                IsFullAccess = this.GetUserRolesPair().HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MerchantVolatilitySettingsList(MerchantVolatilitySettingsListViewModel vm)
        {
            var settingsList = new List<VolatilitySettingsResponse>();

            try
            {
                VolatilitySettingsResponse setting =
                    await _payMerchantClient.Settings.GetVolatilitySettingsAsync(vm.SelectedMerchant);

                if (setting != null)
                    settingsList.Add(setting);
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
            }

            var viewmodel = new MerchantVolatilitySettingsListViewModel
            {
                Settings = settingsList,
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult AddOrEditMerchantVolatilitySettingDialog(AddOrEditMerchantVolatilitySettingsDialog vm)
        {
            bool isNew = string.IsNullOrEmpty(vm.ZeroCoverageAssetPairs);

            vm.Caption = isNew ? "Add setting" : "Edit setting";
            vm.IsNew = isNew;

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchantVolatilitySetting(AddOrEditMerchantVolatilitySettingsDialog vm)
        {
            if (string.IsNullOrEmpty(vm.ZeroCoverageAssetPairs))
                return this.JsonFailResult("ZeroCoverageAssetPairs required", ErrorMessageAnchor);

            if (vm.IsNew)
            {
                var newSettings = new AddVolatilitySettingsRequest
                {
                    MerchantId = vm.MerchantId,
                    ZeroCoverageAssetPairs = vm.ZeroCoverageAssetPairs,
                    IsDeltaSpreadFixed = vm.IsDeltaSpreadFixed
                };

                try
                {
                    await _payMerchantClient.Settings.AddVolatilitySettingsAsync(newSettings);
                }
                catch (ClientApiException e)
                {
                    return this.JsonFailResult(e.Message, ErrorMessageAnchor);
                }
            }
            else
            {
                var editSettings = new UpdateVolatilitySettingsRequest
                {
                    MerchantId = vm.MerchantId,
                    ZeroCoverageAssetPairs = vm.ZeroCoverageAssetPairs,
                    IsDeltaSpreadFixed = vm.IsDeltaSpreadFixed
                };

                try
                {
                    await _payMerchantClient.Settings.UpdateVolatilitySettingsAsync(editSettings);
                }
                catch (ClientApiException e)
                {
                    return this.JsonFailResult(e.Message, ErrorMessageAnchor);
                }
            }

            return this.JsonRequestResult("#merchantVolatilitySettingsList",
                Url.Action("MerchantVolatilitySettingsList"),
                new MerchantVolatilitySettingsListViewModel {SelectedMerchant = vm.MerchantId});
        }
        #endregion

        #region Transfer settings

        [HttpPost]
        public IActionResult DeleteMerchantTransferSettingsDialog(string merchantId, string ruleId, string ruleDisplayName)
        {
            var viewModel = new DeleteMerchantTransferSettingsDialogViewModel
            {
                Caption = "Delete rule from merchant validation settings",
                MerchantId = merchantId,
                RuleId = ruleId,
                RuleDisplayName = ruleDisplayName
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMerchantTransferSettings(DeleteMerchantTransferSettingsDialogViewModel vm)
        {
            try
            {
                await _payTransferValidationClient.Config.DeleteAsync(vm.MerchantId, vm.RuleId);
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return this.JsonFailResult(e.Message, ErrorMessageAnchor);
            }

            return this.JsonRequestResult("#merchantTransferSettingsList",
                Url.Action("MerchantTransferSettingsList", new {SelectedMerchant = vm.MerchantId}));
        }

        [HttpPost]
        public async Task<IActionResult> MerchantTransferSettingsPage()
        {
            var model = new MerchantTransferSettingsListViewModel
            {
                Merchants = await _payMerchantClient.Api.GetAllAsync(),
                CurrentPage = 1,
                IsFullAccess = this.GetUserRolesPair().HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MerchantTransferSettingsList(MerchantTransferSettingsListViewModel vm)
        {
            ConfigurationModel settings = await _payTransferValidationClient.Config.GetAsync(vm.SelectedMerchant);

            var viewmodel = new MerchantTransferSettingsListViewModel
            {
                Settings = settings.Rules,
                IsEditAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsEdit),
                IsFullAccess = (this.GetUserRolesPair()).HasAccessToFeature(UserFeatureAccess.LykkePayMerchantsFull),
                SelectedMerchant = vm.SelectedMerchant
            };

            return View(viewmodel);
        }

        [HttpPost]
        public async Task<ActionResult> AddOrEditMerchantTransferSettingDialog(AddOrEditMerchantTransferSettingsDialog vm)
        {
            bool isNew = string.IsNullOrEmpty(vm.RuleId);

            vm.Caption = isNew ? "Add rule" : "Edit rule";

            vm.IsNew = isNew;

            if (isNew)
            {
                vm.Enabled = true;
            }
            else
            {
                ConfigurationModel cfg = await _payTransferValidationClient.Config.GetAsync(vm.MerchantId);

                vm.RuleInput = cfg.Rules.SingleOrDefault(x => x.RuleId == vm.RuleId)?.RuleInput;
            }

            vm.RuleList = await _payTransferValidationClient.Api.GetRegisteredRules();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditMerchantTransferSetting(AddOrEditMerchantTransferSettingsDialog vm)
        {
            try
            {
                JObject ruleInput = string.IsNullOrEmpty(vm.RuleInput?.Trim()) ? null : JObject.Parse(vm.RuleInput);

                if (vm.IsNew)
                {
                    await _payTransferValidationClient.Config.AddAsync(new AddLineModel
                    {
                        MerchantId = vm.MerchantId,
                        RuleId = vm.RuleId,
                        RuleInput = ruleInput
                    });
                }
                else
                {
                    await _payTransferValidationClient.Config.UpdateInputAsync(new UpdateInputModel
                    {
                        MerchantId = vm.MerchantId,
                        RuleId = vm.RuleId,
                        RuleInput = ruleInput
                    });
                }
            }
            catch (JsonReaderException)
            {
                return this.JsonFailResult("Input must be valid json object", ErrorMessageAnchor);
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest ||
                                               e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return this.JsonFailResult(e.Message, ErrorMessageAnchor);
            }
            catch (ClientApiException e)
            {
                _log.Error(e, "PayTransfer API Call error", $"merchantId = {vm.MerchantId}, ruleId = {vm.RuleId}");

                return this.JsonFailResult("Unexpected API call error", ErrorMessageAnchor);
            }

            return this.JsonRequestResult("#merchantTransferSettingsList",
                Url.Action("MerchantTransferSettingsList"),
                new MerchantTransferSettingsListViewModel {SelectedMerchant = vm.MerchantId});
        }

        [HttpPost]
        public async Task<IActionResult> EnableRule(string merchantId, string ruleId)
        {
            try
            {
                await _payTransferValidationClient.Config.EnableAsync(merchantId, ruleId);

                return this.JsonRequestResult("#merchantTransferSettingsList",
                    Url.Action("MerchantTransferSettingsList",
                        new MerchantTransferSettingsListViewModel { SelectedMerchant = merchantId }));
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return this.JsonFailResult(e.Message, ErrorMessageAnchor);
            }
            catch (ClientApiException e)
            {
                _log.Error(e, "PayTransfer API Call error", $"merchantId: {merchantId}, ruleId = {ruleId}");

                return this.JsonFailResult("Unexpected API call error", ErrorMessageAnchor);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DisableRule(string merchantId, string ruleId)
        {
            try
            {
                await _payTransferValidationClient.Config.DisableAsync(merchantId, ruleId);

                return this.JsonRequestResult("#merchantTransferSettingsList",
                    Url.Action("MerchantTransferSettingsList",
                        new MerchantTransferSettingsListViewModel { SelectedMerchant = merchantId }));
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return this.JsonFailResult(e.Message, ErrorMessageAnchor);
            }
            catch (ClientApiException e)
            {
                _log.Error(e, "PayTransfer API Call error", $"merchantId: {merchantId}, ruleId = {ruleId}");

                return this.JsonFailResult("Unexpected API call error", ErrorMessageAnchor);
            }
        }

        #endregion

        #region Base settings

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
            catch (Lykke.Service.PayInvoice.Client.ErrorResponseException ex) when (ex.StatusCode ==
                                                                                    HttpStatusCode.NotFound)
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
            var pageCount = Convert.ToInt32(Math.Ceiling((double) list.Count() / vm.PageSize));
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
        public ActionResult AddOrEditMerchantSettingDialog(AddOrEditMerchantSettingDialog vm)
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
            catch (Lykke.Service.PayInvoice.Client.ErrorResponseException ex)
            {
                return this.JsonFailResult(ex.Error.ErrorMessage, ErrorMessageAnchor);
            }

            return this.JsonRequestResult("#merchantsSettingsList", Url.Action("MerchantsSettingsList"),
                new MerchantSettingsListViewModel() {SelectedMerchant = vm.MerchantId});
        }

        [HttpPost]
        public async Task<ActionResult> GenerateMerchantCertificatesDialog(string merchantId)
        {
            var merchant = await _payMerchantClient.Api.GetByIdAsync(merchantId);
            var viewModel = new MerchantCertificateViewModel()
            {
                Caption = "Generate merchant certificates",
                MerchantId = merchant.Id,
                MerchantDisplayName = merchant.DisplayName
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> GenerateMerchantCertificates(MerchantCertificateViewModel vm)
        {
            GenerateRsaKeysResponse certs = await _payAuthClient.GenerateRsaKeysAsync(new GenerateRsaKeysRequest
            {
                ClientId = vm.MerchantId,
                ClientDisplayName = vm.MerchantDisplayName
            });

            var guidOfZip = StringUtils.GenerateId();
            var publicKeyPath = Path.Combine(Path.GetTempPath(), $"{StringUtils.GenerateId()}.crt");
            var privateKeyPath = Path.Combine(Path.GetTempPath(), $"{StringUtils.GenerateId()}.pem");
            var zipFilePath = Path.Combine(Path.GetTempPath(), $"{guidOfZip}.zip");

            using (var sw = new StreamWriter(publicKeyPath))
                sw.WriteLine(certs.PublicKey);

            using (var sw = new StreamWriter(privateKeyPath))
                sw.WriteLine(certs.PrivateKey);

            using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(publicKeyPath, $"rsa-public-key_{vm.MerchantDisplayName}.crt");
                zip.CreateEntryFromFile(privateKeyPath, $"rsa-private-key_{vm.MerchantDisplayName}.pem");
            }

            if (System.IO.File.Exists(publicKeyPath))
                System.IO.File.Delete(publicKeyPath);

            if (System.IO.File.Exists(privateKeyPath))
                System.IO.File.Delete(privateKeyPath);

            return Json(guidOfZip);
        }

        [HttpGet]
        public IActionResult DownloadCertificate(string guidOfZip, string merchantDisplayName)
        {
            if (!guidOfZip.IsGuid())
                return BadRequest("Guid should be provided.");

            try
            {
                var zipFilePath = Path.Combine(Path.GetTempPath(), $"{guidOfZip}.zip");

                var ms = new MemoryStream();

                using (var file = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                {
                    file.CopyTo(ms);
                }

                if (System.IO.File.Exists(zipFilePath))
                    System.IO.File.Delete(zipFilePath);

                return File(ms.ToArray(), "application/zip", $"certificates_{merchantDisplayName}.zip");
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error for data {guidOfZip}, {merchantDisplayName}");

                return BadRequest("Error occured.");
            }
        }


        #endregion
    }
}
