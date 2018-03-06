
using BackOffice.Filters;
using BackOffice.Models;
using BackOffice.Translates;
using Core.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Areas.Users.Controllers
{
    [Authorize]
    [Area("Users")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuUsers)]
    public class MenuController : Controller
    {
        [HttpPost]
        public ActionResult Index()
        {
            var viewModel = AreaMenuViewModel.Create(
                AreaMenuItem.Create(Phrases.Users, Url.Action("Index", "Management")),
                AreaMenuItem.Create(Phrases.Roles, Url.Action("Index", "Roles"))
                );

            return View("ViewAreaMenu", viewModel);
        }
    }
}