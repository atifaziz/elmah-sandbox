namespace Elmah.SignalR.Test
{
    #region Imports

    using System.Collections;
    using System.Collections.Generic;
    using System.Web;

    #endregion

    public interface IErrorsStorePersistor : IEnumerable<ErrorsSource>
    {
        void Add(string key, ErrorsSource source);
        ErrorsSource this[string key] { get; }
    }

    public class HttpApplicationErrorsStorePersistor : IErrorsStorePersistor
    {
        private readonly HttpContext context;
        IDictionary<string, ErrorsSource> Sources { get { return (IDictionary<string, ErrorsSource>)context.Application["_sources"]; } }

        public HttpApplicationErrorsStorePersistor(HttpContext context)
        {
            context.Application.Add("_sources", new Dictionary<string, ErrorsSource>());
            this.context = context;
        }

        public void Add(string key, ErrorsSource source)
        {
            Sources.Add(key, source);
        }

        public ErrorsSource this[string key]
        {
            get { return Sources.ContainsKey(key) ? Sources[key] : null; }
        }

        public IEnumerator<ErrorsSource> GetEnumerator()
        {
            return Sources.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MemoryErrorsStorePersistor : IErrorsStorePersistor
    {
        private readonly Dictionary<string, ErrorsSource> _sources = new Dictionary<string, ErrorsSource>();

        public MemoryErrorsStorePersistor(HttpContext context)
        {
            
        }

        public void Add(string key, ErrorsSource source)
        {
            _sources.Add(key, source);
        }

        public ErrorsSource this[string key]
        {
            get { return _sources.ContainsKey(key) ? _sources[key] : null; }
        }

        public IEnumerator<ErrorsSource> GetEnumerator()
        {
            return _sources.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}