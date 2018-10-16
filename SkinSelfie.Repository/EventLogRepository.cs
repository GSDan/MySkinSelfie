using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkinSelfie.ServiceData.Interfaces;
using SkinSelfie.ServiceData.Models;

namespace SkinSelfie.Repository
{
    public class EventLogRepository : IReadWriteRepository<ServiceData.Models.EventLog>
    {
        private readonly SkinSelfieDB context;

        public EventLogRepository()
        {
            context = new SkinSelfieDB();
        }

        public async Task Delete(int id)
        {
            context.EventLogs.Remove(context.EventLogs.Single(s => s.Id == id));
            await context.SaveChangesAsync();
        }

        public IQueryable<ServiceData.Models.EventLog> GetAll()
        {
            return context.EventLogs.Select(s => new ServiceData.Models.EventLog
            {
                Id = s.Id,
                Action = s.Action,
                CreatedAt = s.CreatedAt,
                UserId = s.UserId 
            });
        }

        public ServiceData.Models.EventLog GetById(int id)
        {
            throw new NotImplementedException();
        }

        public ServiceData.Models.EventLog Insert(ServiceData.Models.EventLog model)
        {
            EventLog log = context.EventLogs.Add(new EventLog
            {
                Action = model.Action,
                CreatedAt = model.CreatedAt,
                UserId = model.UserId
            });

            context.SaveChanges();

            model.Id = log.Id;

            return model;
        }

        public IQueryable<ServiceData.Models.EventLog> Search(System.Linq.Expressions.Expression<Func<ServiceData.Models.EventLog, bool>> predicate)
        {
            IQueryable<ServiceData.Models.EventLog> all = GetAll();
            return all.Where(predicate);
        }

        public ServiceData.Models.EventLog Update(ServiceData.Models.EventLog model)
        {
            throw new NotImplementedException();
        }
    }
}
