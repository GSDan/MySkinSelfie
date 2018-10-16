using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
namespace SkinSelfie.WebAPI.Controllers
{
    [Authorize]
    public class StudiesController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.EventLog> _logRepository;
        private readonly IReadWriteRepository<ServiceData.Models.User> _userRepository;
        private readonly IReadWriteRepository<ServiceData.Models.StudyEnrolment> _studyEnrolmentRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Study> _studyRepository;

        public StudiesController(IReadWriteRepository<ServiceData.Models.EventLog> logRepository,
            IReadWriteRepository<ServiceData.Models.User> userRepository,
            IReadWriteRepository<ServiceData.Models.StudyEnrolment> studyEnrolmentRepository,
            IReadWriteRepository<ServiceData.Models.Study> studyRepository)
        {
            _logRepository = logRepository;
            _userRepository = userRepository;
            _studyEnrolmentRepository = studyEnrolmentRepository;
            _studyRepository = studyRepository;
        }

        [Route("api/studies/myenrols")]
        [HttpGet]
        public HttpResponseMessage GetMyEnrols()
        {
            List<ServiceData.Models.StudyEnrolment> found = _studyEnrolmentRepository.Search(
                en => en.User.Email == User.Identity.Name).ToList();

            List<Models.StudyEnrolment> toRet = new List<Models.StudyEnrolment>();
            foreach (ServiceData.Models.StudyEnrolment en in found)
            {
                // Clear out some potentially sensitive info
                Models.StudyEnrolment toAdd = Models.StudyEnrolment.ToAppModel(en, true, false);
                toAdd.Study.StudyEnrolments = null;
                toAdd.Study.Manager.Conditions = null;
                toRet.Add(toAdd);
            }
            return Request.CreateResponse(HttpStatusCode.OK, toRet);
        }

        [Route("api/studies/enrol")]
        [HttpPost]
        public HttpResponseMessage Enrol(string code)
        {
            ServerUtils.LogTelemetryEvent(User.Identity.Name, "Enrol");

            ServiceData.Models.Study found = _studyRepository.Search(
                st => st.Code == code
                ).FirstOrDefault();

            ServiceData.Models.User thisUser = _userRepository.Search(
                usr => usr.Email == User.Identity.Name
                ).FirstOrDefault();

            if (found != null && thisUser != null &&
                !found.StudyEnrolments.Any(en => en.UserId == thisUser.Id))
            {
                Models.StudyEnrolment enrol = new Models.StudyEnrolment
                {
                    CreatedAt = DateTime.UtcNow,
                    StudyId = found.Id,
                    Enrolled = true,
                    UserId = thisUser.Id
                };

                var serviceMod = Models.StudyEnrolment.ToServiceModel(enrol, true, true);
                var finalRes = _studyEnrolmentRepository.Insert(serviceMod);
                return Request.CreateResponse(HttpStatusCode.OK, finalRes);
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [Route("api/studies/unenrol")]
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(int studyId)
        {
            ServiceData.Models.StudyEnrolment found = _studyEnrolmentRepository.Search(
                en => en.User.Email == User.Identity.Name &&
                en.StudyId == studyId
                ).FirstOrDefault();

            if (found != null)
            {
                await _studyEnrolmentRepository.Delete(found.Id);
            }

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "DeleteShare");
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}