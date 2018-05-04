using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using BackOffice.Filters;
using BackOffice.Models;
using BackOffice.Services;
using Common.Log;
using Core.BackOffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Controllers
{
    [Authorize]
    
    public class BackOfficeController : Controller
    {
        private readonly IMenuBadgesRepository _menuBadgesRepository;
        private readonly ILog _log;

        public BackOfficeController(
            IMenuBadgesRepository menuBadgesRepository,
            ILog log)
        {
            _menuBadgesRepository = menuBadgesRepository;
            _log = log;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult Layout()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Menu()
        {
            var viewModel = new MainMenuViewModel
            {
                Ver = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                UserRolesPair = await this.GetUserRolesPair()
            };
            return View(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> GetBadges()
        {
            var badges = (await _menuBadgesRepository.GetBadesAsync()).ToList();

            return Json(badges.Select(itm => new { id = itm.Id, value = itm.Value }));
        }
    }
}
