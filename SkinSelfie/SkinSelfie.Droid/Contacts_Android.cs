using Android.Content;
using Android.Database;
using Android.Provider;
using SkinSelfie.AppModels;
using SkinSelfie.Droid;
using SkinSelfie.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Contacts_Android))]
namespace SkinSelfie.Droid
{
    public class Contacts_Android : IContact
    {
        public static int RequestCode = 111;
        public static bool ResultReady = false;
        public static string ResEmail;

        public async Task<List<EmailContact>> GetEmailContacts()
        {
            List<EmailContact> contacts = new List<EmailContact>();

            MainActivity appMain = Forms.Context as MainActivity;
            ContentResolver cr = appMain.ContentResolver;
            await Task.Run(() =>
            {
                ICursor cur = cr.Query(ContactsContract.Contacts.ContentUri, null, null, null, null);
                if (cur.Count > 0)
                {
                    while (cur.MoveToNext())
                    {
                        String id = cur.GetString(cur.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));
                        ICursor cur1 = cr.Query(
                                ContactsContract.CommonDataKinds.Email.ContentUri, null,
                                ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + " = ?",
                                        new String[] { id }, null);
                        while (cur1.MoveToNext())
                        {
                            //to get the contact names
                            String name = cur1.GetString(cur1.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName));
                            String email = cur1.GetString(cur1.GetColumnIndex(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data));
                            if (email != null)
                            {
                                contacts.Add(new EmailContact { Name = name, Email = email });
                            }
                        }
                        cur1.Close();
                    }
                }
            });
            return contacts;
        }
    }
}

