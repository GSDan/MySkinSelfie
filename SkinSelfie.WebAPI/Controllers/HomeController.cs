using SkinSelfie.WebAPI.Controllers.Site;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SkinSelfie.WebAPI.Controllers
{
    public class HomeController : SiteBaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";
            await LoadViewBag();

            return View();
        }
    }
}
