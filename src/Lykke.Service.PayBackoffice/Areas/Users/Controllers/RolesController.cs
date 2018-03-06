using System.Threading.Tasks;
using System.Net;
using BackOffice.Areas.Users.Models;
using BackOffice.Controllers;
using BackOffice.Filters;
using BackOffice.Translates;
using Core.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Areas.Users.Controllers
{

    [Authorize]
    [Area("Users")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuUsers)]
    public class RolesController : Controller
    {
        private readonly IBackofficeUserRolesRepository _userRolesRepository;

        public RolesController(IBackofficeUserRolesRepository userRolesRepository)
        {
            _userRolesRepository = userRolesRepository;
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            var viewModel = new UserRolesIndexViewModels
            {
                UserRoles = await _userRolesRepository.GetAllRolesAsync(),
                IsCurrentUserAdmin = (await this.GetBackofficeUserAsync()).IsAdmin
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<ActionResult> EditDialog(string id)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            var viewModel = new EditUserRoleUserVewModel
            {
                UserRole = id == null ? BackofficeUserRole.Default : await _userRolesRepository.GetAsync(id),
                Caption = Phrases.EditRole
                
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditUserRoleModel data)
        {
            if (!(await this.GetBackofficeUserAsync()).IsAdmin)
            {
                return StatusCode((int) HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrEmpty(data.Name))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#name");

            if (data.Features == null)
                return this.JsonFailResult(Phrases.PleaseSelectAtLeastOneItem, "#features");

            await _userRolesRepository.SaveAsync(data);

            return this.JsonResultReloadData();
        }

    }
}