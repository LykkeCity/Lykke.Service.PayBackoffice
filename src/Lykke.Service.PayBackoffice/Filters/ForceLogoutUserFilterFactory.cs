using System;
using Core.Users;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BackOffice.Filters
{
    public class ForceLogoutUserFilterFactory : IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var usersRepository = serviceProvider.GetService<IBackOfficeUsersRepository>();

            return new ForceLogoutUserFilter(usersRepository);
        }
    }
}