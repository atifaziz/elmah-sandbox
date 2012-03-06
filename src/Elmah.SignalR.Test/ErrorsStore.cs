namespace Elmah.SignalR.Test
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    #endregion

    // Implementing an errors store, where things
    // like errors persistence and sources activation
    // are handled. This implementation draft works
    // on static member, this should be enhanced
    // to other mechanisms.

    public class ErrorsStore : IEnumerable<ErrorsSource>
    {
        private static ErrorsStore _store;

        private readonly Dictionary<string, ErrorsSource> _sources = new Dictionary<string, ErrorsSource>();
        private int _counter = 0;

        private ErrorsStore()
        {   
        }

        public static ErrorsStore Store
        {
            get { return _store; }
        }

        public static ErrorsStore BuildSourcesFromConfig(HttpContext context)
        {
            _store = new ErrorsStore();
            var sections = (ElmahRSection[])context.GetSection("elmahr");
            foreach (var section in sections)
                _store.AddSource(section.ApplicationName, section.HandshakeToken);
            return _store;
        }

        public ErrorsStore AddSource(string applicationName, string handshakeToken)
        {
            if (this[handshakeToken] != null)
                throw new ArgumentException("This application is already registered.", handshakeToken);

            var source = new ErrorsSource(applicationName, handshakeToken, ++_counter);
            _sources.Add(source.HandshakeToken, source);
            return this;
        }

        public ErrorsSource this[string handshakeToken]
        {
            get
            {
                return _sources.ContainsKey(handshakeToken) ? _sources[handshakeToken] : null;    
            }
        }

        public IEnumerator<ErrorsSource> GetEnumerator()
        {
            return _sources.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool HasSource(string handshakeToken)
        {
            return this[handshakeToken] != null;
        }

        public Error GetError(string id)
        {
            return _sources.Values.Select(source => source.GetError(id))
                                  .FirstOrDefault(error => error != null);
        }
    }

    public class ErrorsSource : IEnumerable<Error>
    {
        private readonly string _applicationName;
        private readonly string _handshakeToken;
        private readonly int _id;
        private string _infoUrl;

        private readonly List<Error> _errors = new List<Error>();
        private readonly Dictionary<string, Error> _errorsById = new Dictionary<string, Error>();

        public ErrorsSource(string applicationName, string handshakeToken, int id)
        {
            _applicationName = applicationName;
            _handshakeToken = handshakeToken;
            _id = id;
        }

        public int Id
        {
            get { return _id; }
        }

        public string HandshakeToken
        {
            get { return _handshakeToken; }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
        }

        public string InfoUrl
        {
            get { return _infoUrl; }
        }

        public ErrorsSource AppendError(Error error, string errorId)
        {
            if (!string.IsNullOrWhiteSpace(error.WebHostHtmlMessage))
            {
                error.Url = "YellowScreenOfDeath.ashx?id=" + errorId;
                error.HasYsod = true;
            }

            _errors.Add(error);
            _errorsById.Add(errorId, error);
            return this;
        }

        public IEnumerator<Error> GetEnumerator()
        {
            return _errors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Error GetError(string id)
        {
            if (_errorsById.ContainsKey(id))
                return _errorsById[id];
            return null;
        }

        public void SetInfoUrl(string infoUrl)
        {
            _infoUrl = infoUrl;
        }
    }
}