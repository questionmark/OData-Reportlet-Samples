using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using QM.Reporting.ODataDashboard.Web.App_Start;

namespace QM.Reporting.ODataDashboard.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", 
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
                );
        }

        protected void Application_BeginRequest()
        {
            // These three lines ensure the back button does not
            // allow the user to see pages after logging out
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Headers.Remove("X-Frame-Options");
        }
    }
}