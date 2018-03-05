using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BackOffice.Areas.Users.Models;
using BackOffice.Controllers;
using BackOffice.Filters;
using BackOffice.Settings;
using BackOffice.Translates;
using Core.Settings;
using Core.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Areas.Users.Controllers
{
    [Authorize]
    [Area("Users")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuUsers)]
    public class ManagementController : Controller
    {
        private readonly IBackOfficeUsersRepository _backOfficeUsersRepository;
        private readonly IBackofficeUserRolesRepository _backofficeUserRolesRepository;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly TwoFactorVerificationSettingsEx _twoFactorVerificationSettings;

        public ManagementController(IBackOfficeUsersRepository backOfficeUsersRepository, 
            IBackofficeUserRolesRepository backofficeUserRolesRepository,
            GoogleAuthSettings googleAuthSettings,
            TwoFactorVerificationSettingsEx twoFactorVerificationSettings)
        {
            _backOfficeUsersRepository = backOfficeUsersRepository;
            _backofficeUserRolesRepository = backofficeUserRolesRepository;
            _googleAuthSettings = googleAuthSettings;
            _twoFactorVerificationSettings = twoFactorVerificationSettings;
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            var viewModel = new UsersManagementIndexViewModel
            {
                IsCurrentUserAdmin = (await this.GetBackofficeUserAsync()).IsAdmin,
                Users = await _backOfficeUsersRepository.GetAllAsync(),
                Roles = (await _backofficeUserRolesRepository.GetAllRolesAsync()).ToDictionary(itm => itm.Id),
                DefaultAdminEmail = _googleAuthSettings.DefaultAdminEmail
            };

            return View(viewModel);
        }


        #region Edit

        [HttpPost]
        public async Task<ActionResult> EditUserDialog(string id)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            EditUserModel editUserModel;

            if (string.IsNullOrEmpty(id))
            {
                editUserModel = EditUserModel.CreateDefault();
                editUserModel.TwoFactorVerificationTrustedTimeSpan = _twoFactorVerificationSettings.TrustedTimeSpan;
            }
            else
            {
                editUserModel = EditUserModel.Map(await _backOfficeUsersRepository.GetAsync(id));
            }

            var viewModel = new EditUserDialogViewModel
            {
                User = editUserModel,
                Caption = Phrases.EditUser,
                Roles = (await _backofficeUserRolesRepository.GetAllRolesAsync()).ToDictionary(itm => itm.Id),
                Width = "900px",
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> EditUser(EditUserModel model)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrEmpty(model.Id))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#id");

            if (!Regex.IsMatch(model.Id, _googleAuthSettings.AvailableEmailsRegex))
                return this.JsonFailResult(Phrases.EmailShouldBeAtLykke, "#id");

            if (string.IsNullOrEmpty(model.FullName))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#fullName");

            if (!string.IsNullOrEmpty(model.TwoFactorVerificationTrustedTimeSpan) &&
                !TimeSpan.TryParse(model.TwoFactorVerificationTrustedTimeSpan, out TimeSpan trustedTimeSpan))
            {
                return this.JsonFailResult(Phrases.InvalidTimeSpanFormat, "#twoFactorVerificationTrustedTimeSpan");
            }

            if (!string.IsNullOrEmpty(model.Create))
            {
                // IF we create new account we have to check whether user exists
                if (await _backOfficeUsersRepository.UserExists(model.Id))
                    return this.JsonFailResult(Phrases.UserExists, "#id");

                await _backOfficeUsersRepository.CreateAsync(model.Id, model.FullName, !string.IsNullOrEmpty(model.IsAdminChecked), model.Roles);
            }
            else
            {
                await _backOfficeUsersRepository.UpdateAsync(model.Id, model.FullName, !string.IsNullOrEmpty(model.IsAdminChecked), model.Roles);
            }

            if (model.HasGoogleAuthenticatorChecked != "on")
            {
                await _backOfficeUsersRepository.ResetGoogleAuthenticatorPrivateKey(model.Id);
            }

            await _backOfficeUsersRepository.SetUseTwoFactorVerification(model.Id, model.UseTwoFactorVerificationChecked == "on");

            string trustedTimeSpanValue = string.IsNullOrEmpty(model.TwoFactorVerificationTrustedTimeSpan)
                ? ""
                : trustedTimeSpan.ToString();

            await _backOfficeUsersRepository.SetUseTwoFactorVerificationTrustedTimeSpan(model.Id, trustedTimeSpanValue);

            return this.JsonRequestResult("#pamain", Url.Action("Index"));
        }

        #endregion


        #region Logout

        [HttpPost]
        public async Task<ActionResult> LogoutUserDialog(string id)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            var viewModel = new UserDialogViewModel
            {
                Id = id,
                Caption = Phrases.LogoutUser,
                Width = "500px"
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> LogoutUser(DoActionWithUserModel model)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrEmpty(model.Id))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmLogoutUser");

            if (!(await _backOfficeUsersRepository.UserExists(model.Id)))
                return this.JsonFailResult(Phrases.UserNotRegistered, "#frmLogoutUser");

            await _backOfficeUsersRepository.ForceLogoutAsync(model.Id);

            return this.JsonRequestResult("#pamain", Url.Action("Index"));
        }

        #endregion


        #region Disable

        [HttpPost]
        public async Task<ActionResult> DisableUserDialog(string id)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var viewModel = new UserDialogViewModel
            {
                Id = id,
                Caption = Phrases.DisableUser,
                Width = "500px"
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> DisableUser(DoActionWithUserModel model)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrEmpty(model.Id))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmDisableUser");

            if (!(await _backOfficeUsersRepository.UserExists(model.Id)))
                return this.JsonFailResult(Phrases.UserNotRegistered, "#frmDisableUser");

            await _backOfficeUsersRepository.DisableAsync(model.Id);

            return this.JsonRequestResult("#pamain", Url.Action("Index"));
        }

        #endregion


        #region Enable

        [HttpPost]
        public async Task<ActionResult> EnableUserDialog(string id)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var viewModel = new UserDialogViewModel
            {
                Id = id,
                Caption = Phrases.EnableUser,
                Width = "500px"
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> EnableUser(DoActionWithUserModel model)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrEmpty(model.Id))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmEnableUser");

            if (!(await _backOfficeUsersRepository.UserExists(model.Id)))
                return this.JsonFailResult(Phrases.UserNotRegistered, "#frmEnableUser");

            await _backOfficeUsersRepository.EnableAsync(model.Id);

            return this.JsonRequestResult("#pamain", Url.Action("Index"));
        }

        #endregion


        #region Remove

        [HttpPost]
        public async Task<ActionResult> RemoveUserDialog(string id)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            var viewModel = new UserDialogViewModel
            {
                Id = id,
                Caption = Phrases.RemoveUser,
                Width = "500px"
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> RemoveUser(RemoveUserModel model)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrEmpty(model.Id))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#frmRemoveUser");

            if (string.IsNullOrEmpty(model.IdConfirmation))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#idConfirmation");

            if (model.Id != model.IdConfirmation)
                return this.JsonFailResult(Phrases.EmailConfirmationDoesntMatch, "#idConfirmation");

            if (!(await _backOfficeUsersRepository.UserExists(model.Id)))
                return this.JsonFailResult(Phrases.UserNotRegistered, "#frmRemoveUser");

            await _backOfficeUsersRepository.RemoveAsync(model.Id);

            return this.JsonRequestResult("#pamain", Url.Action("Index"));
        }

        #endregion
    }

}
