using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BhFS.Tippspiel.Utils;

namespace WMTippApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            // log4net
            {
                log4net.Config.XmlConfigurator.Configure();
            }

            log.Debug("Start Application begin");

            AreaRegistration.RegisterAllAreas();

            log.Debug("Register WebApi");
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            log.Debug("Register Filters");
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            log.Debug("Register Routes");
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            log.Debug("Register Bundles");
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log.Debug("Register Auth");
            AuthConfig.RegisterAuth();

            SportsdataConfigInfo.Current.LeagueShortcut = ConfigurationManager.AppSettings["LeagueShortcut"];
            SportsdataConfigInfo.Current.LeagueSaison = ConfigurationManager.AppSettings["LeagueSaison"];

            log.Debug("Start Application end");
        }
    }
}