using System.Collections.Generic;
using BackOffice.Models;
using Core.Users;

namespace BackOffice.Areas.Users.Models
{
    public class UserRolesIndexViewModels
    {
        public IEnumerable<IBackofficeUserRole> UserRoles { get; set; }
        public bool IsCurrentUserAdmin { get; set; }
    }

    public class EditUserRoleUserVewModel : IPersonalAreaDialog
    {
        public IBackofficeUserRole UserRole { get; set; }
        public string Caption { get; set; }
        public string Width { get; set; }
    }


    public class EditUserRoleModel : IBackofficeUserRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public UserFeatureAccess[] Features { get; set; }
    }
}
