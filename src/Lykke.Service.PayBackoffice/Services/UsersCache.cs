using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Users;

namespace BackOffice.Services
{
    public static class UsersCache
    {

        private static DateTime _lastCacheUpdated = new DateTime(2010,1,1);
        private static Dictionary<string, IBackOfficeUser> _usersCache = new Dictionary<string, IBackOfficeUser>();
        private static IBackofficeUserRole[] _rolesCache = new IBackofficeUserRole[0];


        public static async Task UpdateUsersAndRoles()
        {
            if ((DateTime.UtcNow - _lastCacheUpdated).TotalSeconds > 30)
            {
                _usersCache = (await Dependencies.BackOfficeUsersRepository.GetAllAsync()).ToDictionary(itm => itm.Id);

                _rolesCache =
                    (await Dependencies.BackofficeUserRolesRepository.GetAllRolesAsync()).Select(
                        BackofficeUserRole.Create).Cast<IBackofficeUserRole>().ToArray();

                _lastCacheUpdated = DateTime.UtcNow;
            }
        }


        public static UserRolesPair GetUsersRolePair(string userId)
        {


            if (!_usersCache.ContainsKey(userId))
                return new UserRolesPair
                {
                    User = null,
                    Roles = null
                };

            return new UserRolesPair
            {
                User = _usersCache[userId],
                Roles = _rolesCache
            };
        }

    }
}
