using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Practices.Unity;
using SkinSelfie.Repository;
using SkinSelfie.ServiceData.Interfaces;
using SkinSelfie.WebAPI.Controllers;
using SkinSelfie.WebAPI.Models;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using Unity.WebApi;

namespace SkinSelfie.WebAPI
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<DbContext, ApplicationDbContext>(new HierarchicalLifetimeManager());
            container.RegisterType<UserManager<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<AccountController>(new InjectionConstructor());
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(o => HttpContext.Current.GetOwinContext().Authentication));

            container.RegisterType<IReadWriteRepository<ServiceData.Models.Photo>, PhotoRepository>();
            container.RegisterType<IReadWriteRepository<ServiceData.Models.User>, UserRepository>();
            container.RegisterType<IReadWriteRepository<ServiceData.Models.UserCondition>, UserConditionsRepository>();
            container.RegisterType<IReadOnlyRepository<ServiceData.Models.BodyPart>, BodyPartRepository>();
            container.RegisterType<IReadWriteRepository<ServiceData.Models.EventLog>, EventLogRepository>();
            container.RegisterType<IReadWriteRepository<ServiceData.Models.Share>, ShareRepository>();
            container.RegisterType<IReadWriteRepository<ServiceData.Models.StudyEnrolment>, StudyEnrolmentRepository>();
            container.RegisterType<IReadWriteRepository<ServiceData.Models.Study>, StudyRepository>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}