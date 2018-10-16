using SkinSelfie.ServiceData.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SkinSelfie.ServiceData.Interfaces
{
    public interface IReadOnlyRepository<T> where T : Model
    {
        T GetById(int id);
        IQueryable<T> Search(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAll();
    }
}
