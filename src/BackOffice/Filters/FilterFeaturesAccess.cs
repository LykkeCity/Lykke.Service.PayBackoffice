
using BackOffice.Services;
using Core.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackOffice.Filters
{
    public class FilterFeaturesAccessAttribute : ActionFilterAttribute
    {
        private readonly UserFeatureAccess[] _filters;

        public FilterFeaturesAccessAttribute(params UserFeatureAccess[] filters)
        {
            _filters = filters;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var userId = filterContext.HttpContext.User.Identity.Name;

            if (userId == null)
            {
                
                filterContext.Result = new StatusCodeResult(403);
            }
            else
            {

                var userPair = UsersCache.GetUsersRolePair(userId);

                foreach (var userFeatureAccess in _filters)
                {
                    if (userPair.HasAccessToFeature(userFeatureAccess))
                    {
                        base.OnActionExecuting(filterContext);
                        return;
                    }
                }

                filterContext.Result =  new StatusCodeResult(403);
            }
        }
    }
}
