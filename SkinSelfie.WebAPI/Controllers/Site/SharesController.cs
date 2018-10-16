using SkinSelfie.Repository;
using SkinSelfie.ServiceData.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SkinSelfie.WebAPI.Controllers.Site
{
    [Authorize]
    public class SharesController : SiteBaseController
    {
        // GET: Shares
        public async Task<ActionResult> Index()
        {
            await LoadViewBag();
            IReadWriteRepository<ServiceData.Models.Share> _shareRepository = new ShareRepository();

            List<ServiceData.Models.Share> found =
                _shareRepository.Search(sh => sh.SharedEmail == User.Identity.Name).ToList();
            List<Models.Share> toRet = new List<Models.Share>();
            foreach (ServiceData.Models.Share sh in found)
            {
                toRet.Add(Models.Share.ToAppModel(sh, true));
            }

            toRet = toRet.OrderByDescending(item => 
                item.UserCondition.Photos.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt).ToList();

            ViewData["Title"] = "Shared With Me";
           
            return View(toRet);
        }
    }
}