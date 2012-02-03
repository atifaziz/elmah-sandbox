namespace Elmah.Sandbox
{
    #region Imports
    
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Linq;
    using System.Web;

    #endregion

    public class ErrorPostModule : HttpModuleBase
    {
        private Uri _url;

        protected override void OnInit(HttpApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            //
            // Get the configuration section of this module.
            // If it's not there then there is nothing to initialize or do.
            // In this case, the module is as good as mute.
            //

            var config = (IDictionary)GetConfig();
            if (config == null)
                return;

            _url = new Uri(GetSetting(config, "url"), UriKind.Absolute);

            var modules = application.Modules;
            var errorLogModule = Enumerable.Range(0, modules.Count)
                                           .Select(i => modules[i])
                                           .OfType<ErrorLogModule>()
                                           .SingleOrDefault();

            if (errorLogModule != null)
                errorLogModule.Logged += OnErrorLogged;
        }

        protected virtual void OnErrorLogged(object sender, ErrorLoggedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            SetError(HttpContext.Current, args.Entry.Error);
        }

        private static void SetError(HttpContext context, Error error)
        {
            
        }

        #region Configuration

        internal const string GroupName = "elmah";
        internal const string GroupSlash = GroupName + "/";

        public static object GetSubsection(string name)
        {
            return GetSection(GroupSlash + name);
        }

        public static object GetSection(string name)
        {
            return ConfigurationManager.GetSection(name);
        }

        protected virtual object GetConfig()
        {
            return GetSubsection("errorPost");
        }

        private static string GetSetting(IDictionary config, string name)
        {
            System.Diagnostics.Debug.Assert(config != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name));

            string value = ((string)config[name]) ?? string.Empty;

            if (value.Length == 0)
            {
                throw new Elmah.ApplicationException(string.Format(
                    "The required configuration setting '{0}' is missing for the error tweeting module.", name));
            }

            return value;
        }

        #endregion
    }
}
