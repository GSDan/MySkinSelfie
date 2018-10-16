using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinSelfie.AppModels;
using SkinSelfie.Interfaces;
using SkinSelfie.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(Contacts_iOS))]
namespace SkinSelfie.iOS
{
    public class Contacts_iOS : IContact
    {
        public async Task<List<EmailContact>> GetEmailContacts()
        {
            Xamarin.Contacts.AddressBook book = new Xamarin.Contacts.AddressBook();
            List<EmailContact> contacts = new List<EmailContact>();

            await book.RequestPermission().ContinueWith(t =>
            {
                if (!t.Result)
                {
                    Console.WriteLine("Permission denied by user or manifest");
                    return;
                }
                foreach (var contact in book.Where(c => c.Emails.Any()))
                {
					var firstOrDefault = contact.Emails.FirstOrDefault();
					if (firstOrDefault != null)
                    {
                        contacts.Add(new EmailContact
                        {
                            Email = firstOrDefault.Address,
                            Name = contact.FirstName + " " + contact.LastName
                        });
                    }
                }
            });

            contacts = contacts.OrderByDescending(c => c.Name).ToList();
            return contacts;
        }
    }
}
