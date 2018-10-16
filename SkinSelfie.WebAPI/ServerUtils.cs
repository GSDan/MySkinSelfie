using Microsoft.ApplicationInsights;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SendGrid;
using SendGrid.Helpers.Mail;
using ServiceData;
using System;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace SkinSelfie.WebAPI
{
    public static class ServerUtils
    {
        public static void LogTelemetryEvent(string userId, string eventName)
        {
            var tc = new TelemetryClient();
            tc.Context.User.Id = userId;
            tc.TrackEvent(eventName);
        }

        //https://azure.microsoft.com/en-gb/documentation/articles/key-vault-use-from-web-application/
        //this is an optional property to hold the secret after it is retrieved
        public static string EncryptSecret { get; set; }

        //the method that will be provided to the KeyVaultClient
        // Issues with async code after 1 hour - have to make sure that the entire pipeline using this 
        // method is properly using async (including controllers)
        // https://github.com/Azure/azure-sdk-for-net/issues/1432
        public async static Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(WebConfigurationManager.AppSettings["ClientId"],
                        WebConfigurationManager.AppSettings["ClientSecret"]);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        public static async Task SendEmail(string email, string name, string subject, string htmlMessage)
        {
            SendGridMessage mail = new SendGridMessage()
            {
                From = new EmailAddress(ConfidentialData.SendGridEmailAdd, "MySkinSelfie"),
                Subject = subject,
                HtmlContent = htmlMessage
            };

            mail.AddTo(new EmailAddress(email, name));

            SendGridClient client = new SendGridClient(ConfidentialData.SendGridApiKey);
            await client.SendEmailAsync(mail);
        }
    }
}