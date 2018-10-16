using SkinSelfie.ServiceData.Models;
using System.Threading.Tasks;

namespace SkinSelfie.ServiceData.Interfaces
{
    public interface IReadWriteRepository<T> : IReadOnlyRepository<T> where T : Model
    {
        T Insert(T model);
        T Update(T model);
        Task Delete(int id);
    }
}
