using SkinSelfie.Repository;
using SkinSelfie.ServiceData.Interfaces;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SkinSelfie.WebAPI.Controllers.Site
{
    public class SiteBaseController : Controller
    {
        protected async Task LoadViewBag()
        {
            ViewBag.IsAdmin = (User.Identity.IsAuthenticated) ? await IsAdmin() : false;
        }

        protected async Task<bool> IsAdmin()
        {
            IReadWriteRepository<ServiceData.Models.User> _userRepository = new UserRepository();
            ServiceData.Models.User thisUser =
                await _userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefaultAsync();

            return thisUser?.Admin == true;
        }
    }
}