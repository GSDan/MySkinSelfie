using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceData;

namespace SkinSelfie.WebAPI.Controllers
{
    public class EventLogController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.EventLog> _logRepository;
        private readonly IReadWriteRepository<ServiceData.Models.User> _userRepository;

        public EventLogController(IReadWriteRepository<ServiceData.Models.EventLog> logRepository, IReadWriteRepository<ServiceData.Models.User> userRepository)
        {
            _logRepository = logRepository;
            _userRepository = userRepository;
        }

        // GET api/<controller>
        [AllowAnonymous]
        public HttpResponseMessage Get(string password)
        {
            if(password != ConfidentialData.AdminPassword)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var results = _logRepository.GetAll().OrderByDescending(l => l.CreatedAt).Select(l => new Models.EventLog
            {
                Id = l.Id,
                Action = l.Action,
                CreatedAt = l.CreatedAt,
                UserId = l.UserId
            });

            return Request.CreateResponse(HttpStatusCode.OK, results);
        }

        [AllowAnonymous]
        public HttpResponseMessage Get(string password, string email)
        {
            if (password != ConfidentialData.AdminPassword)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            ServiceData.Models.User user = _userRepository.Search(u => u.Email == email).FirstOrDefault();

            var results = _logRepository.Search(log => log.UserId == user.Id).OrderByDescending(l => l.CreatedAt).Select(l => new Models.EventLog
            {
                Id = l.Id,
                Action = l.Action,
                CreatedAt = l.CreatedAt,
                UserId = l.UserId
            });

            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}