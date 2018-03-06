using System.Collections.Generic;
using BackOffice.Models;
using Core.Users;

namespace BackOffice.Areas.Users.Models
{
    public class UsersManagementIndexViewModel
    {
        public bool IsCurrentUserAdmin { get; set; }
        public IEnumerable<IBackOfficeUser> Users { get; set; }
        public Dictionary<string, IBackofficeUserRole> Roles { get; set; }
        public string DefaultAdminEmail { get; set; }
    }


    public class EditUserDialogViewModel : IPersonalAreaDialog
    {
        public EditUserModel User { get; set; }
        public Dictionary<string, IBackofficeUserRole> Roles { get; set; } 
        public string Caption { get; set; }
        public string Width { get; set; }
    }

    public class UserDialogViewModel : IPersonalAreaDialog
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Width { get; set; }
    }

    public class EditUserModel
    {
        public string Create { get; set; }
        public string Id { get; set; }
        public string FullName { get; set; }
        public string[] Roles { get; set; }
        public string IsAdminChecked { get; set; }
        public bool IsAdmin { get; set; }
        public string HasGoogleAuthenticatorChecked { get; set; }
        public bool HasGoogleAuthenticator { get; set; }
        public string UseTwoFactorVerificationChecked { get; set; }
        public bool UseTwoFactorVerification { get; set; }
        public string TwoFactorVerificationTrustedTimeSpan { get; set; }

        public static EditUserModel CreateDefault()
        {
            return new EditUserModel
            {
                IsAdminChecked = string.Empty,
                Roles = new string[0]
            };
        }

        public static EditUserModel Map(IBackOfficeUser user)
        {
            return new EditUserModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Roles = user.Roles,
                IsAdmin = user.IsAdmin,
                HasGoogleAuthenticator = user.GoogleAuthenticatorConfirmBinding,
                UseTwoFactorVerification = user.UseTwoFactorVerification,
                TwoFactorVerificationTrustedTimeSpan = user.TwoFactorVerificationTrustedTimeSpan
            };
        }
    }

    public class DoActionWithUserModel
    {
        public string Id { get; set; }
    }

    public class RemoveUserModel
    {
        public string Id { get; set; }
        public string IdConfirmation { get; set; }
    }
}
