using BackOffice.Helpers;
using BackOffice.Models;
using BackOffice.Services;
using BackOffice.Settings;
using BackOffice.Translates;
using Core.Settings;
using Google.Apis.Auth;
using Lykke.GoogleAuthenticator;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.BackofficeMembership.Client.AutorestClient.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.BackofficeMembership.Client.Models;

namespace BackOffice.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBackofficeMembershipClient _backofficeMembershipClient;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly BackOfficeTwoFactorVerificationSettings _twoFactorVerificationSettings;
        private readonly ILog _log;

        public HomeController(IBackofficeMembershipClient backofficeMembershipClient,
            GoogleAuthSettings googleAuthSettings,
            BackOfficeTwoFactorVerificationSettings twoFactorVerificationSettings,
            ILogFactory logFactory)
        {
            _backofficeMembershipClient = backofficeMembershipClient;
            _googleAuthSettings = googleAuthSettings;
            _twoFactorVerificationSettings = twoFactorVerificationSettings;
            _log = logFactory.CreateLog(this);
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
                BrowserSession = await _backofficeMembershipClient.GetBrowserSessionAsync(sessionId),
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

                var authenticationResult = await _backofficeMembershipClient.AuthenticateAsync(
                    new AuthenticationDataModel
                    {
                        UserId = webSignature.Email,
                        Code = data.Code,
                        Ip = this.GetIp(),
                        SessionId = this.GetSession(),
                        UseTwoFactorVerification = _twoFactorVerificationSettings.UseVerification
                    });

                if (authenticationResult.Result == AuthenticationResult.UserNotRegistered
                    || authenticationResult.Result == AuthenticationResult.SecondFactorIsFailed)
                {
                    return this.JsonFailResult(Phrases.UserNotRegistered, "#googleSignIn");
                }

                if (authenticationResult.Result == AuthenticationResult.UserIsDisabled)
                {
                    return this.JsonFailResult(Phrases.UserIsDisabled, "#googleSignIn");
                }

                await SignIn(authenticationResult.User);
            }
            catch (InvalidJwtException ex)
            {
                _log.Info($"Invalid Jwt: {ex}");
                return this.JsonFailResult(Phrases.InvalidJwt, "#googleSignIn");
            }

            var divResult = Request.IsMobileBrowser() ? "#pamain" : "body";

            _log.Info("Authenticate success");

            return this.JsonRequestResult(divResult, Url.Action(nameof(BackOfficeController.Layout), "BackOffice"));
        }

        private static ClaimsIdentity MakeIdentity(BackofficeUserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(ClaimTypes.Email, user.Id)
            };

            return new ClaimsIdentity(claims, "Cookie");
        }


        private async Task SignIn(BackofficeUserModel user)
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

                TwoFactorInfoModel twoFactorInfo = await _backofficeMembershipClient.CheckTwoFactorAsync(
                    new CheckTwoFactorModel()
                    {
                        UserId = email,
                        Ip = this.GetIp()
                    });

                if (twoFactorInfo.Result == CheckTwoFactorResult.UserNotRegistered)
                {
                    _log.Info($"User {email} is not registered.");
                    return this.JsonFailResult(Phrases.UserNotRegistered, "#googleSignIn");
                }

                if (twoFactorInfo.Result == CheckTwoFactorResult.UserIsDisabled)
                {
                    _log.Info($"User {email} is disabled");
                    return this.JsonFailResult(Phrases.UserIsDisabled, "#googleSignIn");
                }

                if (twoFactorInfo.Result == CheckTwoFactorResult.SkipVerification)
                {
                    return Json(new TwoFactorInfo
                    {
                        UseVerification = false
                    });
                }

                return Json(new TwoFactorInfo
                {
                    UseVerification = true,
                    ExistCode = twoFactorInfo.ExistCode,
                    ImageUrl = twoFactorInfo.ImageUrl,
                    TextKey = twoFactorInfo.TextKey
                });
            }
            catch (InvalidJwtException ex)
            {
                _log.Info($"Invalid Jwt: {ex}");
                return this.JsonFailResult(Phrases.InvalidJwt, "#googleSignIn");
            }
        }

        private ActionResult CheckWebSignature(GoogleJsonWebSignatureEx.Payload webSignature)
        {
            if (!string.Equals(webSignature.Audience, _googleAuthSettings.ApiClientId))
            {
                _log.Info($"Invalid Google Api Client Id: {webSignature.Audience}");
                return this.JsonFailResult(Phrases.InvalidGoogleApiClientId, "#googleSignIn");
            }

            if (string.IsNullOrWhiteSpace(webSignature.Email))
            {
                _log.Info("Email Is Empty");
                return this.JsonFailResult(Phrases.EmailIsEmpty, "#googleSignIn");
            }

            if (!Regex.IsMatch(webSignature.Email, _googleAuthSettings.AvailableEmailsRegex))
            {
                _log.Info($"{webSignature.Email} is invalid. Email Should Be At Lykke.");
                return this.JsonFailResult(Phrases.EmailShouldBeAtLykke, "#googleSignIn");
            }

            if (!webSignature.IsEmailValidated)
            {
                _log.Info(webSignature.Email, "Email Is Not Validated");
                return this.JsonFailResult(Phrases.EmailIsNotValidated, "#googleSignIn");
            }

            return null;
        }
    }
}
