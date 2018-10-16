using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SkinSelfie.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            UnityConfig.RegisterComponents();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // https://azure.microsoft.com/en-gb/documentation/articles/key-vault-use-from-web-application/
            //KeyVaultClient kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(ServerUtils.GetToken));
            //KeyBundle sec = kv.GetKeyAsync("https://SkinSelfieVault.vault.azure.net", "FileEncryption").GetAwaiter().GetResult();
            //ServerUtils.EncryptSecret = sec.Key.ToString();
        }
    }
}
