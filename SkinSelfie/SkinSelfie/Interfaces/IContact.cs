using SkinSelfie.AppModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinSelfie.Interfaces
{
    public interface IContact
    {
        /// Returns all contacts with an email address
        Task<List<EmailContact>> GetEmailContacts();
    }
}
