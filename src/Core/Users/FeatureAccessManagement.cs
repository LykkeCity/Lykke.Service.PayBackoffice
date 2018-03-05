using System.Linq;

namespace Core.Users
{
    public struct UserRolesPair
    {
        public IBackOfficeUser User { get; set; }
        public IBackofficeUserRole[] Roles { get; set; }
    } 

    public static class FeatureAccessUtls
    {
        public static bool HasAccessToFeature(this IBackOfficeUser src, IBackofficeUserRole[] roles, UserFeatureAccess feature)
        {
            if (src.IsAdmin)
                return true;

            foreach (var roleId in src.Roles)
            {
                var foundRole = roles.FirstOrDefault(role => role.Id == roleId);
                if (foundRole == null)
                    continue;

                if (foundRole.Features.Any(f => f == feature))
                    return true;

            }

            return false;
        }

        public static bool HasAccessToFeature(this UserRolesPair src, UserFeatureAccess feature)
        {
            return HasAccessToFeature(src.User, src.Roles, feature);
        }
    }
}
