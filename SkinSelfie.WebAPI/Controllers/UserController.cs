using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;

namespace SkinSelfie.WebAPI.Controllers
{

    public class UserController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.User> _userRepository;
        private readonly IReadWriteRepository<ServiceData.Models.UserCondition> _conditionRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Photo> _photoRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Share> _shareRepository;
        private readonly IReadWriteRepository<ServiceData.Models.EventLog> _logRepository;

        public UserController(IReadWriteRepository<ServiceData.Models.User> userRepository,
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

        // GET api/values/5
        public HttpResponseMessage Get(int id)
        {
            ServiceData.Models.User found = _userRepository.GetById(id);

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (found.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            Models.User toRet = Models.User.ToAppModel(found);

            if (toRet.BirthDate == null || (DateTime.UtcNow - toRet.BirthDate).TotalDays < 2)
            {
                toRet.BirthDate = DateTime.UtcNow.AddYears(-30);
            }

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetUser");
            PostLog("Users_GetSingle", found.Id);
            return Request.CreateResponse(HttpStatusCode.OK, toRet);
        }

        public HttpResponseMessage Get(string email)
        {
            Models.User found = Models.User.ToAppModel(_userRepository.Search(user => user.Email == email).FirstOrDefault());

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (found.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            if (found.BirthDate == null || (DateTime.UtcNow - found.BirthDate).TotalDays < 2)
            {
                found.BirthDate = DateTime.UtcNow.AddYears(-30);
            }

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetUserWithEmail");
            PostLog("Users_GetByEmail", found.Id);
            return Request.CreateResponse(HttpStatusCode.OK, found);
        }

        struct UserStat
        {
            public string Initials;
            public string DoB;
            public int NumAlbums;
            public int NumPhotos;
        }

        //[AllowAnonymous]
        //public HttpResponseMessage Get()
        //{
        //    List<ServiceData.Models.User> found = _userRepository.GetAll().ToList();
        //    List<UserStat> toRet = new List<UserStat>();

        //    Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");

        //    foreach (ServiceData.Models.User u in found)
        //    {
        //        if (u.BirthDate == null || (DateTime.UtcNow - u.BirthDate).TotalDays < 2)
        //        {
        //            u.BirthDate = DateTime.UtcNow.AddYears(-30);
        //        }

        //        UserStat thisStat = new UserStat
        //        {
        //            Initials = initials.Replace(u.Name, "$1"),
        //            DoB = u.BirthDate.ToString("yyyy-MM-dd"),
        //            NumAlbums = 0,
        //            NumPhotos = 0
        //        };

        //        foreach(ServiceData.Models.UserCondition c in u.Conditions)
        //        {
        //            thisStat.NumAlbums++;
        //            thisStat.NumPhotos += c.Photos.Count();
        //        }

        //        toRet.Add(thisStat);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, toRet);
        //}

        // POST api/values
        public HttpResponseMessage Post([FromBody]Models.User newUser)
        {
            try
            {
                newUser.Email = User.Identity.Name; // Name = email

                if (newUser.BirthDate == null || (DateTime.UtcNow - newUser.BirthDate).TotalDays < 2)
                {
                    newUser.BirthDate = DateTime.UtcNow.AddYears(-30);
                }

                Models.User returned = Models.User.ToAppModel(_userRepository.Insert(Models.User.ToServiceModel(newUser)));

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "CreateUser");
                PostLog("Users_Create", returned.Id);

                return Request.CreateResponse(HttpStatusCode.OK, returned);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        // PUT api/values/5
        public HttpResponseMessage Put(int id, [FromBody]Models.User updatedUser)
        {
            updatedUser.Id = id;

            Models.User found = Models.User.ToAppModel(_userRepository.Search(user => user.Email == updatedUser.Email).FirstOrDefault());

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (found.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            if (updatedUser.BirthDate == null)
            {
                updatedUser.BirthDate = DateTime.UtcNow.AddYears(-30);
            }

            ServiceData.Models.User final = _userRepository.Update(Models.User.ToServiceModel(updatedUser));

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "UpdateUser");
            PostLog("Users_Update", final.Id);
            return Request.CreateResponse(HttpStatusCode.OK, Models.User.ToAppModel(final));
        }

        public async Task<HttpResponseMessage> Delete()
        {
            try
            {
                ServiceData.Models.User found = _userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefault();
                if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
                if (found.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

                // Delete all conditions and photos from DB and storage
                foreach (ServiceData.Models.UserCondition condition in found.Conditions)
                {
                    await UserConditionsController.Delete(_conditionRepository, _shareRepository, _photoRepository, condition.Id);
                }

                PostLog("Users_Delete", found.Id);

                await _userRepository.Delete(found.Id);

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "DeleteUser");

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

        }
    }
}
