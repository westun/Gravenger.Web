using AutoMapper;
using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Security;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Gravenger
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<MappingProfile>());

            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
        }

        protected void FormsAuthentication_OnAuthenticate(Object sender, FormsAuthenticationEventArgs e)
        {
            if (FormsAuthentication.CookiesSupported)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    try
                    {
                        IUnitOfWork unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>();
                        string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                        Account account = unitOfWork.Accounts.GetByUsernameWithRoles(username);
                        if (account != null)
                        {
                            string[] roles = account.Roles?.Select(r => r.Name).ToArray();
                            e.User = new GenericPrincipal(new AccountIdentity(username, "Forms", account.AccountID), roles);
                        }

                    }
                    catch (Exception)
                    {
                        //TODO: Handle exception in global.asax related to authentication
                    }
                }
            }
            else
            {
                throw new HttpException("Cookieless Forms Authentication is not supported for this application.");
            }
        }

    }
}