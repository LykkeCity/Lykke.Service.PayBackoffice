using System.Threading.Tasks;
using Core.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;

namespace BackOffice.Filters
{
    public class ForceLogoutUserFilter : IAsyncActionFilter
    {
        private readonly IBackOfficeUsersRepository _usersRepository;

        public ForceLogoutUserFilter(IBackOfficeUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = context.HttpContext.User.Identity.Name;
                if (userId != null)
                {
                    var user = await _usersRepository.GetAsync(userId);

                    if (user == null || user.IsForceLogoutRequested)
                    {
                        await context.HttpContext.SignOutAsync("Cookie");
                        context.Result = new ForbidResult();

                        return;
                    }
                }
            }

            await next();
        }
    }
}
