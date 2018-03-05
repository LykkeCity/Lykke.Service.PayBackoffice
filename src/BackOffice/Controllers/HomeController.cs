﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackOffice.Helpers;
using BackOffice.Models;
using BackOffice.Services;
using BackOffice.Settings;
using BackOffice.Translates;
using Core;
using Core.AuditLog;
using Core.BitCoin;
using Core.Clients;
using Core.Security;
using Core.Settings;
using Core.Users;
using Google.Apis.Auth;
using LkeServices.Kyc;
using Lykke.GoogleAuthenticator;
using Lykke.Service.ClientAccount.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace BackOffice.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBrowserSessionsRepository _browserSessionsRepository;
        private readonly IBackOfficeUsersRepository _backOfficeUsersRepository;
        private readonly IClientAccountClient _clientAccountService;
        private readonly ISrvSecurityHelper _srvSecurityHelper;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ITemporaryIdRepository _temporaryIdRepository;
        private readonly IPinSecurityRepository _pinSecurityRepository;
        private readonly IClientSigningService _clientSigningService;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly TwoFactorVerificationSettingsEx _twoFactorVerificationSettings;

        public HomeController(IBrowserSessionsRepository browserSessionsRepository,
            IBackOfficeUsersRepository backOfficeUsersRepository,
            ISrvSecurityHelper srvSecurityHelper, IWalletCredentialsRepository walletCredentialsRepository,
            IAuditLogRepository auditLogRepository, ITemporaryIdRepository temporaryIdRepository,
            IPinSecurityRepository pinSecurityRepository, IClientAccountClient clientAccountService,
            IClientSigningService clientSigningService,
            GoogleAuthSettings googleAuthSettings,
            TwoFactorVerificationSettingsEx twoFactorVerificationSettings)
        {
            _browserSessionsRepository = browserSessionsRepository;
            _backOfficeUsersRepository = backOfficeUsersRepository;
            _clientAccountService = clientAccountService;
            _srvSecurityHelper = srvSecurityHelper;
            _walletCredentialsRepository = walletCredentialsRepository;
            _auditLogRepository = auditLogRepository;
            _temporaryIdRepository = temporaryIdRepository;
            _pinSecurityRepository = pinSecurityRepository;
            _clientSigningService = clientSigningService;
            _googleAuthSettings = googleAuthSettings;
            _twoFactorVerificationSettings = twoFactorVerificationSettings;
        }

        public async Task<ActionResult> Index(string langId)
        {


            if (langId != null)
                this.SetLanguage(langId);


            if (User.Identity.IsAuthenticated)
                return View("IndexAuthenticated");

            var sessionId = this.GetSession();

            var viewModel = new IndexPageModel
            {
                BrowserSession = await _browserSessionsRepository.GetSessionAsync(sessionId),
                GoogleApiClientId = _googleAuthSettings.ApiClientId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Authenticate(AuthenticateModel data)
        {
            try
            {
                var webSignature = await GoogleJsonWebSignatureEx.ValidateAsync(data.GoogleSignInIdToken);

                ActionResult checkError = CheckWebSignature(webSignature);

                if (checkError != null)
                    return checkError;

                IBackOfficeUser user = await _backOfficeUsersRepository.GetAsync(webSignature.Email);

                if (user == null)
                    return this.JsonFailResult(Phrases.UserNotRegistered, "#googleSignIn");

                if (user.IsDisabled)
                    return this.JsonFailResult(Phrases.UserIsDisabled, "#googleSignIn");

                if (_twoFactorVerificationSettings.UseVerification && user.UseTwoFactorVerification)
                {
                    if (!IsTrustedIp(user.TwoFactorVerificationTrustedIPs, this.GetIp()))
                    {
                        if (!CheckTwoFactorAuthenticator(user.GoogleAuthenticatorPrivateKey, data.Code))
                            return this.JsonFailResult(Phrases.UserNotRegistered, "#googleSignIn");

                        if (!TimeSpan.TryParse(user.TwoFactorVerificationTrustedTimeSpan, out TimeSpan trustedTimeSpan))
                            trustedTimeSpan = TimeSpan.Parse(_twoFactorVerificationSettings.TrustedTimeSpan);

                        string ips = AddTrustedIp(user.TwoFactorVerificationTrustedIPs, this.GetIp(), trustedTimeSpan);

                        await _backOfficeUsersRepository.SetTrustedIpAddresses(user.Id, ips);
                    }

                    if (!user.GoogleAuthenticatorConfirmBinding)
                        await _backOfficeUsersRepository.SetGoogleAuthenticatorConfirmBinding(user.Id, true);
                }

                await _backOfficeUsersRepository.ResetForceLogoutAsync(user.Id);

                var sessionId = this.GetSession();

                await _browserSessionsRepository.SaveSessionAsync(sessionId, user.Id);

                await SignIn(user);
            }
            catch (InvalidJwtException)
            {
                return this.JsonFailResult(Phrases.InvalidJwt, "#googleSignIn");
            }

            var divResult = Request.IsMobileBrowser() ? "#pamain" : "body";

            return this.JsonRequestResult(divResult, Url.Action(nameof(BackOfficeController.Layout), "BackOffice"));
        }

        private static ClaimsIdentity MakeIdentity(IBackOfficeUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(ClaimTypes.Email, user.Id)
            };

            return new ClaimsIdentity(claims, "Cookie");
        }


        private async Task SignIn(IBackOfficeUser user)
        {
            var identity = MakeIdentity(user);
            await HttpContext.SignInAsync("Cookie", new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
        }

        public ActionResult GoogleSignout()
        {
            return View((object)_googleAuthSettings.ApiClientId);
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookie");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Version()
        {
            return new JsonResult(new { Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion });
        }

        public async Task<ActionResult> ChangePassword(string id)
        {
            var clientId = id != null ? await _temporaryIdRepository.GetRealId(id) : null;

            var client = !string.IsNullOrEmpty(clientId) ? await _clientAccountService.GetByIdAsync(clientId) : null;

            if (client == null)
                return RedirectToAction("Result", new { message = "Change password link is inactive or incorrect." });

            var vm = new ChangePasswordVm
            {
                TemporaryId = id,
                Email = client.Email
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordVm vm)
        {
            var clientId = vm.TemporaryId != null ? await _temporaryIdRepository.GetRealId(vm.TemporaryId) : null;

            var client = !string.IsNullOrEmpty(clientId) ? await _clientAccountService.GetByIdAsync(clientId) : null;

            if (client == null)
                return RedirectToAction("Result", new { message = "Client not found" });

            if (vm.Password != vm.ConfirmPassword)
            {
                ModelState.AddModelError("Password", "Not equal");
                ModelState.AddModelError("ConfirmPassword", "Not equal");
            }

            if (vm.PIN != vm.ConfirmPIN)
            {
                ModelState.AddModelError("PIN", "Not equal");
                ModelState.AddModelError("ConfirmPIN", "Not equal");
            }

            if (ModelState.IsValid)
            {
                await _clientAccountService.ChangeClientPasswordAsync(clientId, vm.Password);

                if (vm.PIN != null)
                {
                    var pin = vm.PIN.Value.ToString("0000");
                    await _pinSecurityRepository.SaveAsync(clientId, pin);
                    await _clientAccountService.SetPinAsync(clientId, pin);
                    await _auditLogRepository.AddOtherEventAsync(clientId, "PIN changed.", RecordChanger.Client);
                }

                var walletCredsBefore = await _walletCredentialsRepository.GetAsync(clientId);

                if (walletCredsBefore != null)
                {
                    var pk = await _clientSigningService.GetPrivateKey(walletCredsBefore.Address);
                    var encodedPrivateKey = _srvSecurityHelper.EncodePrivateKey(pk, vm.Password);
                    await _walletCredentialsRepository.SetEncodedPrivateKey(clientId, encodedPrivateKey);
                    await _clientAccountService.SetHashedAsync(clientId, false);
                    await _auditLogRepository.AddOtherEventAsync(clientId, "Password changed.", RecordChanger.Client);
                }

                await _temporaryIdRepository.RemoveTemporaryIdRecord(vm.TemporaryId);

                return RedirectToAction("Result", new { message = "Password was changed." });
            }

            return View(vm);
        }

        public async Task<ActionResult> ChangePin(string id)
        {
            var clientId = id != null ? await _temporaryIdRepository.GetRealId(id) : null;

            var client = !string.IsNullOrEmpty(clientId) ? await _clientAccountService.GetByIdAsync(clientId) : null;

            if (client == null)
                return RedirectToAction("Result", new { message = "Change PIN link is inactive or incorrect." });

            var vm = new ChangePin
            {
                TemporaryId = id,
                Email = client.Email
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> ChangePin(ChangePin vm)
        {
            var clientId = vm.TemporaryId != null ? await _temporaryIdRepository.GetRealId(vm.TemporaryId) : null;

            var client = !string.IsNullOrEmpty(clientId) ? await _clientAccountService.GetByIdAsync(clientId) : null;

            if (client == null)
                return RedirectToAction("Result", new { message = "Client not found" });

            if (vm.PIN == null)
            {
                ModelState.AddModelError("PIN", "Please, enter PIN");
            }

            if (vm.PIN != vm.ConfirmPIN)
            {
                ModelState.AddModelError("PIN", "Not equal");
                ModelState.AddModelError("ConfirmPIN", "Not equal");
            }

            if (ModelState.IsValid)
            {
                var pin = vm.PIN?.ToString("0000");
                await _pinSecurityRepository.SaveAsync(clientId, pin);
                await _clientAccountService.SetPinAsync(clientId, pin);

                await _auditLogRepository.AddOtherEventAsync(clientId, "PIN changed.", RecordChanger.Client);

                return RedirectToAction("Result", new { message = "Your PIN was changed." });
            }

            return View(vm);
        }

        public ActionResult Result(string message)
        {
            return View("Result", message);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> CheckTwoFactor(AuthenticateModel data)
        {
            if (!_twoFactorVerificationSettings.UseVerification)
            {
                return Json(new TwoFactorInfo
                {
                    UseVerification = false
                });
            }

            try
            {
                var webSignature = await GoogleJsonWebSignatureEx.ValidateAsync(data.GoogleSignInIdToken);

                var checkError = CheckWebSignature(webSignature);

                if (checkError != null)
                {
                    return checkError;
                }

                string email = webSignature.Email;

                IBackOfficeUser user = await _backOfficeUsersRepository.GetAsync(email);

                if (user == null)
                {
                    return this.JsonFailResult(Phrases.UserNotRegistered, "#googleSignIn");
                }

                if (user.IsDisabled)
                {
                    return this.JsonFailResult(Phrases.UserIsDisabled, "#googleSignIn");
                }

                if (user.UseTwoFactorVerification)
                {
                    var res = new TwoFactorInfo
                    {
                        ExistCode = user.GoogleAuthenticatorConfirmBinding,
                        UseVerification = !IsTrustedIp(user.TwoFactorVerificationTrustedIPs, this.GetIp())
                    };

                    if (!res.ExistCode)
                    {
                        await _backOfficeUsersRepository.GenerateGoogleAuthenticatorPrivateKey(user.Id);
                        user = await _backOfficeUsersRepository.GetAsync(email);

                        var setupInfo =
                            GenerateTwoFactorAuthenticatorSetupCode(user.Id, user.GoogleAuthenticatorPrivateKey);

                        res.ImageUrl = setupInfo.QrCodeSetupImageUrl;
                        res.TextKey = setupInfo.ManualEntryKey;
                    }
                    return Json(res);
                }

                return Json(new TwoFactorInfo
                {
                    UseVerification = false
                });
            }
            catch (InvalidJwtException)
            {
                return this.JsonFailResult(Phrases.InvalidJwt, "#googleSignIn");
            }
        }

        private ActionResult CheckWebSignature(GoogleJsonWebSignatureEx.Payload webSignature)
        {
            if (!string.Equals(webSignature.Audience, _googleAuthSettings.ApiClientId))
            {
                return this.JsonFailResult(Phrases.InvalidGoogleApiClientId, "#googleSignIn");
            }

            if (string.IsNullOrWhiteSpace(webSignature.Email))
            {
                return this.JsonFailResult(Phrases.EmailIsEmpty, "#googleSignIn");
            }

            if (!Regex.IsMatch(webSignature.Email, _googleAuthSettings.AvailableEmailsRegex))
            {
                return this.JsonFailResult(Phrases.EmailShouldBeAtLykke, "#googleSignIn");
            }

            if (!webSignature.IsEmailValidated)
            {
                return this.JsonFailResult(Phrases.EmailIsNotValidated, "#googleSignIn");
            }

            return null;
        }

        private bool CheckTwoFactorAuthenticator(string privateKey, string code)
        {
            var tfa = new TwoFactorAuthenticator();
            var twoFactorResult = tfa.ValidateTwoFactorPIN(privateKey, code);

            return twoFactorResult;
        }

        private SetupCode GenerateTwoFactorAuthenticatorSetupCode(string userId, string privateKey)
        {
            var tfa = new TwoFactorAuthenticator();

            var prefix = !string.IsNullOrEmpty(_twoFactorVerificationSettings.AppNamePrefix)
                ? _twoFactorVerificationSettings.AppNamePrefix + "."
                : "";


            var setupInfo = tfa.GenerateSetupCode(prefix + "LykkeWallet.BackOffice",
                userId, privateKey, 200, 200, true);

            return setupInfo;
        }

        #region Methods for working with user trusted ip addresses

        private static bool IsTrustedIp(string collection, string ip)
        {
            if (string.IsNullOrEmpty(collection))
                return false;

            List<Tuple<string, DateTime>> list = ParceTrustedIpList(collection);

            return !IsExpiredIp(ip, DateTime.UtcNow, list);
        }

        private static string AddTrustedIp(string collection, string ip, TimeSpan trustedTimeSpan)
        {
            List<Tuple<string, DateTime>> list = ParceTrustedIpList(collection);

            DateTime dateTime = DateTime.UtcNow;

            list = list.Where(o => !string.Equals(o.Item1, ip) && !IsExpiredIp(o.Item1, dateTime, list))
                .ToList();

            list.Add(new Tuple<string, DateTime>(ip, dateTime.Add(trustedTimeSpan)));

            return string.Join("&", list.Select(o => $"{o.Item1}|{o.Item2:u}"));
        }

        private static List<Tuple<string, DateTime>> ParceTrustedIpList(string value)
        {
            var result = new List<Tuple<string, DateTime>>();

            if (string.IsNullOrEmpty(value))
                return result;

            foreach (string pair in value.Split("&"))
            {
                string[] values = pair.Split("|");

                if(values.Length != 2)
                    continue;

                if(!DateTime.TryParseExact(values[1], "u", null, DateTimeStyles.AdjustToUniversal, out DateTime experitionDate))
                    continue;

                result.Add(new Tuple<string, DateTime>(values[0], experitionDate));
            }

            return result;
        }

        private static bool IsExpiredIp(string ip, DateTime dateTime, List<Tuple<string, DateTime>> list)
        {
            var trustedIp = list.FirstOrDefault(o => o.Item1 == ip);

            return trustedIp == null || trustedIp.Item2 <= dateTime;
        }

        #endregion
    }
}
