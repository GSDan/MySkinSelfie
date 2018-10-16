using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceData;

namespace SkinSelfie.WebAPI.Controllers
{
    [Authorize]
    public class SharesController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.Photo> _photoRepository;
        private readonly IReadWriteRepository<ServiceData.Models.UserCondition> _conditionRepository;
        private readonly IReadWriteRepository<ServiceData.Models.EventLog> _logRepository;
        private readonly IReadWriteRepository<ServiceData.Models.User> _userRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Share> _shareRepository;

        public SharesController(IReadWriteRepository<ServiceData.Models.Photo> photoRepository, IReadWriteRepository<ServiceData.Models.UserCondition> conditionRepository, IReadWriteRepository<ServiceData.Models.EventLog> logRepository, IReadWriteRepository<ServiceData.Models.User> userRepository, IReadWriteRepository<ServiceData.Models.Share> shareRepository)
        {
            _photoRepository = photoRepository;
            _conditionRepository = conditionRepository;
            _logRepository = logRepository;
            _userRepository = userRepository;
            _shareRepository = shareRepository;
        }

        public async Task<HttpResponseMessage> Post([FromBody]Models.Share newShare)
        {
            try
            {
                // Does this share already exist? Change the existing share rather than making a new one
                ServiceData.Models.Share found = _shareRepository.Search(sh =>
                                                    sh.UserCondition.Id == newShare.UserCondition.Id &&
                                                    sh.SharedEmail == newShare.SharedEmail).FirstOrDefault();

                Models.Share toRet = null;

                if (found != null)
                {
                    found.ExpireDate = newShare.ExpireDate;
                    _shareRepository.Update(found);
                    toRet = Models.Share.ToAppModel(found, false);
                }
                else
                {
                    newShare.CreatedAt = DateTime.Now;
                    newShare.Updated = true;
                    ServiceData.Models.Share returned = _shareRepository.Insert(Models.Share.ToServiceModel(newShare, true));
                    toRet = Models.Share.ToAppModel(returned, false);

                    await ServerUtils.SendEmail(
                        toRet.SharedEmail,
                        "",
                        "New MySkinSelfie share from " + newShare.UserCondition.Owner.Name,
                        string.Format("{0} has shared their album '{1}' with you." +
                        " Create or log into an account with this email address at {2} to see it!",
                            newShare.UserCondition.Owner.Name,
                            newShare.UserCondition.Condition,
                            string.Format("{0}Conditions/Index/{1}", ConfidentialData.SiteUrl, toRet.UserCondition.Id)));

                }

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "AddShare");
                return Request.CreateResponse(HttpStatusCode.OK, toRet);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [Route("api/shares/sharedwithme")]
        [HttpGet]
        public HttpResponseMessage GetSharedWithMe()
        {
            List<Models.Share> toRet = SharedWith(User.Identity.Name);
            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetSharedWithMe");
            return Request.CreateResponse(HttpStatusCode.OK, toRet);
        }

        public List<Models.Share> SharedWith(string email)
        {
            List<ServiceData.Models.Share> found = _shareRepository.Search(sh => sh.SharedEmail == email).ToList();
            List<Models.Share> toRet = new List<Models.Share>();
            foreach (ServiceData.Models.Share sh in found)
            {
                toRet.Add(Models.Share.ToAppModel(sh, true));
            }
            return toRet;
        }

        [Route("api/shares/myshares")]
        [HttpGet]
        public HttpResponseMessage GetMyShares()
        {
            List<ServiceData.Models.Share> found = _shareRepository.Search(sh => 
                sh.Owner.Email == User.Identity.Name 
                && sh.ExpireDate > DateTime.UtcNow
                ).ToList();

            List<Models.Share> toRet = new List<Models.Share>();
            foreach (ServiceData.Models.Share sh in found)
            {
                toRet.Add(Models.Share.ToAppModel(sh, false));
            }

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetMyShares");
            return Request.CreateResponse(HttpStatusCode.OK, toRet);
        }

        public async Task<HttpResponseMessage> Delete(int id)
        {
            ServiceData.Models.Share found = _shareRepository.GetById(id);

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (found.Owner.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            await _shareRepository.Delete(id);

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "DeleteShare");
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}