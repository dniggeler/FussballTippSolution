using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WMTippApp.Models;

namespace WMTippApp.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeTippspielDBAttribute : ActionFilterAttribute
    {
        private static TippspielDBInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure Tippspiel database is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class TippspielDBInitializer
        {
            public TippspielDBInitializer()
            {
                Database.SetInitializer<TippSpielContext>(null);

                try
                {
                    using (var context = new TippSpielContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the tipp database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Tippspiel database could not be initialized.", ex);
                }
            }
        }
    }
}
