using SkinSelfie.ServiceData.Interfaces;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace SkinSelfie.WebAPI.Controllers
{
    [Authorize]
    public class BodyPartsController : ApiController
    {
        private readonly IReadOnlyRepository<ServiceData.Models.BodyPart> _bpRepository;

        public BodyPartsController(IReadOnlyRepository<ServiceData.Models.BodyPart> bpRepository)
        {
            _bpRepository = bpRepository;
        }

        // GET: api/BodyParts
        public IQueryable<Models.BodyPart> GetBodyParts()
        {
            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetBodyParts");

            return _bpRepository.GetAll().Select(bp => new Models.BodyPart
            {
                Id = bp.Id,
                Name = bp.Name,
                SkinRegions = bp.SkinRegions.Select(sr => new Models.SkinRegion
                {
                    Id = sr.Id,
                    Name = sr.Name,
                }).ToList()
            });
        }

        // GET: api/BodyParts/5
        [ResponseType(typeof(Models.BodyPart))]
        public IHttpActionResult GetBodyPart(int id)
        {
            ServiceData.Models.BodyPart found = _bpRepository.GetById(id);

            if (found == null)
            {
                return NotFound();
            }

            return Ok(Models.BodyPart.ToAppModel(_bpRepository.GetById(id), true));
        }
    }
}