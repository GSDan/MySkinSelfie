using Microsoft.WindowsAzure.Storage.Blob;
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
    public class UserConditionsController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.User> _userRepository;
        private readonly IReadWriteRepository<ServiceData.Models.UserCondition> _conditionRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Photo> _photoRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Share> _shareRepository;
        private readonly IReadWriteRepository<ServiceData.Models.EventLog> _logRepository;
        private Random rnd = new Random();

        public UserConditionsController(IReadWriteRepository<ServiceData.Models.User> userRepository,
                IReadWriteRepository<ServiceData.Models.UserCondition> conditionRepository, 
                IReadWriteRepository<ServiceData.Models.Photo> photoRepository,
                IReadWriteRepository<ServiceData.Models.EventLog> logRepository,
                IReadWriteRepository<ServiceData.Models.Share> shareRepository)
        {
            _userRepository = userRepository;
            _conditionRepository = conditionRepository;
            _photoRepository = photoRepository;
            _shareRepository = shareRepository;
            _logRepository = logRepository;
        }

        private void PostLog(string eventType, int? userId = null)
        {
            if (userId == null)
            {
                var user = _userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefault();
                if (user == null) return;
                userId = user.Id;
            }

            _logRepository.Insert(new ServiceData.Models.EventLog
            {
                Action = eventType,
                CreatedAt = DateTime.Now,
                UserId = (int)userId
            });
        }

        // GET api/values
        public HttpResponseMessage Get()
        {
            var user = _userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefault();

            if(user == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var results = _conditionRepository.Search(c => c.Owner.Email == User.Identity.Name).Select(con => new Models.UserCondition
            {
                Id = con.Id,
                Condition = con.Condition,
                Treatment = con.Treatment,
                StartDate = con.StartDate,
                Passcode = con.Passcode,
                Finished = con.Finished,
                SkinRegion = new Models.SkinRegion
                {
                    Id = con.SkinRegion.Id,
                    Name = con.SkinRegion.Name,
                    BodyPart = new Models.BodyPart
                    {
                        Id = con.SkinRegion.BodyPart.Id,
                        Name = con.SkinRegion.BodyPart.Name
                    }
                },
                Photos = con.Photos.OrderByDescending(p => p.CreatedAt).Select(p => new Models.Photo
                {
                    Id = p.Id,
                    CreatedAt = p.CreatedAt,
                    Url = p.Url,
                    ThumbUrl = p.ThumbUrl,
                    Notes = p.Notes,
                    PhotoDescription = p.PhotoDescription,
                    Rating = p.Rating,
                    Treatment = p.Treatment
                }).ToList()
            });

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetAllConditions");
            PostLog("UserConditions_GetAll", user.Id);
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }

        public HttpResponseMessage Get(int id)
        {
            ServiceData.Models.UserCondition found = _conditionRepository.GetById(id);

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (found.Owner.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            if (found.Photos.Count() > 0)
            {
                found.Photos = found.Photos.OrderByDescending(photo => photo.CreatedAt);
            }
            
            Models.UserCondition toRet = Models.UserCondition.ToAppModel(found, false);

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetCondition");
            PostLog("UserConditions_GetSingle", found.Owner.Id);
            return Request.CreateResponse(HttpStatusCode.OK, toRet);
        }

        // POST api/values
        public HttpResponseMessage Post([FromBody]Models.UserCondition newCondition)
        {
            try
            {
                newCondition.Owner = Models.User.ToAppModel(_userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefault());

                ServiceData.Models.UserCondition returned = _conditionRepository.Insert(Models.UserCondition.ToServiceModel(newCondition, true));

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "AddCondition");
                PostLog("UserConditions_Create", newCondition.Owner.Id);
                return Request.CreateResponse(HttpStatusCode.OK, Models.UserCondition.ToAppModel(returned, false));
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        // PUT api/values/5
        public HttpResponseMessage Put(int id, [FromBody]Models.UserCondition updated)
        {
            updated.Id = id;

            ServiceData.Models.UserCondition found = _conditionRepository.GetById(id);
            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (found.Owner.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            ServiceData.Models.UserCondition final = _conditionRepository.Update(Models.UserCondition.ToServiceModel(updated, false));

            if (final == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "UpdateCondition");
            PostLog("UserConditions_Update", found.Owner.Id);
            return Request.CreateResponse(HttpStatusCode.OK, Models.UserCondition.ToAppModel(final, true));
        }

        public async Task<HttpResponseMessage> ResetPin(int id)
        {
            ServiceData.Models.UserCondition found = _conditionRepository.GetById(id);

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (found.Owner.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            found.Passcode = rnd.Next(1000, 9999);

            ServiceData.Models.UserCondition final = _conditionRepository.Update(found);
            await ServerUtils.SendEmail(found.Owner.Email, found.Owner.Name,
                "Folder PIN Reset",
                string.Format("Hi {0},<br/>Your new PIN for the folder '{1}' has been reset, and is now '{2}'. Feel free to change it in the application.",
                    found.Owner.Name, found.Condition, final.Passcode));

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "ResetConditionPIN");
            PostLog("UserConditions_PINReset", found.Owner.Id);
            return Request.CreateResponse(HttpStatusCode.OK, Models.UserCondition.ToAppModel(final, true));
        }


        // DELETE api/values/5
        public async Task<HttpResponseMessage> Delete(int id)
        {
            try
            {
                ServiceData.Models.UserCondition found = _conditionRepository.GetById(id);
                if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
                if (found.Owner.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

                await Delete(_conditionRepository, _shareRepository, _photoRepository, id);

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "DeleteCondition");
                PostLog("UserConditions_Delete", found.Owner.Id);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        public static async Task Delete(IReadWriteRepository<ServiceData.Models.UserCondition> conditionRep, IReadWriteRepository<ServiceData.Models.Share> shareRep, IReadWriteRepository<ServiceData.Models.Photo> photoRep, int id)
        {
            ServiceData.Models.UserCondition found = conditionRep.GetById(id);
            if (found == null) return;

            ServiceData.Models.Share[] foundShares = shareRep.Search( sh => sh.UserCondition.Id == found.Id ).ToArray();
            foreach(ServiceData.Models.Share share in foundShares)
            {
                await shareRep.Delete(share.Id);
            }

            CloudBlobContainer container = await UploadController.GetBlobContainer();

            foreach (ServiceData.Models.Photo photo in found.Photos)
            {
                await PhotoController.Delete(photoRep, photo.Id);
            }

            await conditionRep.Delete(id);
        }
    }
}
