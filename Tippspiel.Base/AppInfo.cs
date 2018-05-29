using System.Web;

namespace Tippspiel.Base
{
    public class AppInfo<TSubclass> where TSubclass : AppInfo<TSubclass>, new()
    {
        private static string Key
        {
            get { return typeof(AppInfo<TSubclass>).FullName; }
        }

        private static TSubclass Value
        {
            get { return (TSubclass)HttpContext.Current.Application[Key]; }
            set { HttpContext.Current.Application[Key] = value; }
        }

        public static TSubclass Current
        {
            get
            {
                var instance = Value;
                if (instance == null)
                    lock (typeof(TSubclass)) // not ideal to lock on a type -- but it'll work
                    {
                        // standard lock double-check
                        instance = Value;
                        if (instance == null)
                            Value = instance = new TSubclass();
                    }
                return instance;
            }
        }
    }

}
