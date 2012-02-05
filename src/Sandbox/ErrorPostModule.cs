using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Elmah.Sandbox
{
    #region Imports
    
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Linq;
    using System.Web;

    #endregion

    // This module builds a json representation of
    // a trapped error and POSTs it to a destination.
    // So far it does not do anything else, like
    // handling authentication or other stuff.

    public class ErrorPostModule : HttpModuleBase
    {
        private Uri _url;
        private string _applicationName;
        private string _handshakeToken;

        protected override void OnInit(HttpApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            // Get the configuration section of this module.
            // If it's not there then there is nothing to initialize or do.
            // In this case, the module is as good as mute.

            var config = (IDictionary)GetConfig();
            if (config == null)
                return;

            // The module so far is  expecting one parameter,
            // caller 'url', which identifies the destination
            // of the HTTP POST that the module will perform.
            // It also requires an application name and a handshake
            // token which 'authorizes' the application to post
            // to the destination. This is a simple authorization
            // mechanism which does not replace available
            // authentication/authorizations systems.

            _url               = new Uri(GetSetting(config, "url"), UriKind.Absolute);
            _applicationName   = GetOptionalSetting(config, "applicationName");
            _handshakeToken    = GetOptionalSetting(config, "handshakeToken");

            var modules        = application.Modules;
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
            SetError(/* HttpContext.Current, */ args.Entry.Error);
        }

        private void SetError(/* HttpContext context, */ Error e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(_url);
                request.Method = "POST"; 
                request.ContentType = "application/x-www-form-urlencoded";

                // See http://blogs.msdn.com/shitals/archive/2008/12/27/9254245.aspx
                request.ServicePoint.Expect100Continue = false;

                // The idea is to post to an url the json representation
                // of the intercepted error. We do a base 64 encoding
                // to fool the other side just in case some sort of
                // automated post validation is performed (do we have a 
                // better way to do this?). We post also the application
                // name and the handshaking token.

                using (var writer = new StringWriter())
                {
                    ErrorJson.Encode(e, writer);

                    var form = string.Format("error={0}&handshakeToken={1}", 
                        HttpUtility.UrlEncode(Base64Encode(writer.ToString())),
                        _handshakeToken != null  ? HttpUtility.UrlEncode(_handshakeToken)  : string.Empty);

                    // Get the bytes to determine
                    // and set the content length.

                    var data = Encoding.ASCII.GetBytes(form);
                    Debug.Assert(data.Length > 0);
                    request.ContentLength = data.Length;

                    // Post it! (asynchronously)

                    request.BeginGetRequestStream(ar =>
                    {
                        if (ar == null) throw new ArgumentNullException("ar");
                        var args = (object[])ar.AsyncState;
                        OnGetRequestStreamCompleted(ar, (WebRequest)args[0], (byte[])args[1]);                                             
                    }, AsyncArgs(request, data));
                }
            }
            catch (Exception localException)
            {
                // IMPORTANT! We swallow any exception raised during the 
                // logging and send them out to the trace . The idea 
                // here is that logging of exceptions by itself should not 
                // be  critical to the overall operation of the application.
                // The bad thing is that we catch ANY kind of exception, 
                // even system ones and potentially let them slip by.

                OnWebPostError(/* request, */ localException);
            }
        }

        private static object[] AsyncArgs(params object[] args)
        {
            return args;
        }

        private static void OnGetRequestStreamCompleted(IAsyncResult ar, WebRequest request, byte[] data)
        {
            Debug.Assert(ar != null);
            Debug.Assert(request != null);
            Debug.Assert(data != null);
            Debug.Assert(data.Length > 0);

            try
            {
                using (var output = request.EndGetRequestStream(ar))
                    output.Write(data, 0, data.Length);
                request.BeginGetResponse(rar =>
                {
                    if (rar == null) throw new ArgumentNullException("rar");
                    OnGetResponseCompleted(rar, (WebRequest)rar.AsyncState);        
                }, request);
            }
            catch (Exception e)
            {
                OnWebPostError(/* request, */ e);
            }
        }

        private static void OnGetResponseCompleted(IAsyncResult ar, WebRequest request)
        {
            Debug.Assert(ar != null);
            Debug.Assert(request != null);

            try
            {
                Debug.Assert(request != null);
                request.EndGetResponse(ar).Close(); // Not interested; assume OK
            }
            catch (Exception e)
            {
                OnWebPostError(/* request, */ e);
            }
        }

        public static string Base64Encode(string str)
        {
            var encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        private static void OnWebPostError(/* WebRequest request, */ Exception e)
        {
            Debug.Assert(e != null);
            Trace.WriteLine(e);
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
            Debug.Assert(config != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            var value = ((string)config[name]) ?? string.Empty;

            if (value.Length == 0)
            {
                throw new Elmah.ApplicationException(string.Format(
                    "The required configuration setting '{0}' is missing for the error tweeting module.", name));
            }

            return value;
        }

        private static string GetOptionalSetting(IDictionary config, string name, string defaultValue = null)
        {
            Debug.Assert(config != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            var value = ((string)config[name]) ?? string.Empty;

            if (value.Length == 0)
                return defaultValue;

            return value;
        }

        #endregion
    }
}
