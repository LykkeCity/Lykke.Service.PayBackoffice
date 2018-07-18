using BackOffice.Models;
using Common.Log;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Lykke.Common.Log;

namespace BackOffice.Controllers
{
    [Authorize]
    
    public class BackOfficeController : Controller
    {
        private readonly ILog _log;

        public BackOfficeController(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
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
                UserRolesPair = this.GetUserRolesPair()
            };
            return View(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> GetBadges()
        {
            return Json(new object[0]);
        }
    }
}
