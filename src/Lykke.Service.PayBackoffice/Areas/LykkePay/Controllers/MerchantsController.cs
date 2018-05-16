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
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using BackOffice.Areas.LykkePay.Models.Merchants;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

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
            //GenerateSertificate();
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
                Id = id,
                LwId = merchant.LwId,
                Name = merchant.Name,
                PublicKey = merchant.PublicKey,
                TimeCacheRates = merchant.TimeCacheRates,
                Certificate = merchant.PublicKey,
                SystemId = string.Empty,
                DisplayName = merchant.DisplayName,
                IsBlocked = true //TODO: update payinvoice
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
                        LwId = vm.LwId,
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
                    LwId = vm.LwId,
                    TimeCacheRates = vm.TimeCacheRates,
                    Name = vm.Name,
                    DisplayName = vm.DisplayName,
                    //IsBlocked = vm.IsBlocked //TODO: update payinvoice
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

            return this.JsonRequestResult("#MerchantsPage", Url.Action("MerchantsList"));
        }
        [HttpPost]
        public async Task<ActionResult> GenerateMerchantCertificatesDialog(string merchantId)
        {
            var merchant = await _payInternalClient.GetMerchantByIdAsync(merchantId);
            var viewModel = new MerchantCertificateViewModel()
            {
                Caption = "Generate merchant certificates",
                DisplayName = merchant.DisplayName
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<HttpResponseMessage> GenerateMerchantCertificates(MerchantCertificateViewModel vm)
        {
            //var certs = GenerateSertificate(vm);
            //string result = Path.GetTempPath();
            string myTempFile = Path.Combine(Path.GetTempPath(), "SaveFile.txt");
            using (StreamWriter sw = new StreamWriter(myTempFile))
            {
                sw.WriteLine("Your error message");
            }
            var myTempFile1 = Path.Combine(Path.GetTempPath(), "SaveFile1.txt");
            using (StreamWriter sw = new StreamWriter(myTempFile1))
            {
                sw.WriteLine("Your error message");
            }
            using (ZipArchive zip = ZipFile.Open(Path.Combine(Path.GetTempPath(), "certificates.zip"), ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(myTempFile, "SaveFile.txt");
                zip.CreateEntryFromFile(myTempFile1, "SaveFile1.txt");
            }
            MemoryStream ms = new MemoryStream();
            using (FileStream file = new FileStream(Path.Combine(Path.GetTempPath(), "certificates.zip"), FileMode.Open, FileAccess.Read))
                file.CopyTo(ms);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ms.ToArray())
            };
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "certificates.zip"
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }
        protected MerchantCertificate GenerateSertificate(MerchantCertificateViewModel vm)
        {
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();
            g.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            var pair = g.GenerateKeyPair();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(pair.Private);
            byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetEncoded();

            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
            sb.AppendLine(Convert.ToBase64String(serializedPrivateBytes));
            sb.AppendLine("-----END RSA PRIVATE KEY-----");
            var serializedPrivate = sb.ToString();

            var parameters = string.Format("E={0},CN={1},OU={2},O={3},C={4}", vm.Email, vm.DisplayName, vm.OrgUnit, vm.Organization, vm.Country);
            var caName = new X509Name(parameters);
            var caCert = GenerateCertificate(caName, caName, pair.Private, pair.Public);
            var certEncoded = caCert.GetEncoded();
            sb = new StringBuilder();
            sb.AppendLine("-----BEGIN CERTIFICATE-----");
            sb.AppendLine(Convert.ToBase64String(certEncoded));
            sb.AppendLine("-----END CERTIFICATE-----");
            var serializedPublic = sb.ToString();
            return new MerchantCertificate() { Private = serializedPrivate, Public = serializedPublic };


            //SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pair.Public);
            //byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            //string serializedPublic = Convert.ToBase64String(serializedPublicBytes);

            //RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(serializedPrivate));
            //RsaKeyParameters publicKey = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(serializedPublic));
        }
        protected static X509Certificate GenerateCertificate(
        X509Name issuer, X509Name subject,
        AsymmetricKeyParameter issuerPrivate,
        AsymmetricKeyParameter subjectPublic)
        {
            ISignatureFactory signatureFactory;
            signatureFactory = new Asn1SignatureFactory(
                PkcsObjectIdentifiers.Sha256WithRsaEncryption.ToString(),
                issuerPrivate);

            var certGenerator = new X509V3CertificateGenerator();
            certGenerator.SetIssuerDN(issuer);
            certGenerator.SetSubjectDN(subject);
            certGenerator.SetSerialNumber(BigInteger.ValueOf(1));
            certGenerator.SetNotAfter(DateTime.UtcNow.AddHours(1));
            certGenerator.SetNotBefore(DateTime.UtcNow);
            certGenerator.SetPublicKey(subjectPublic);
            return certGenerator.Generate(signatureFactory);
        }

    }
}
