using System;
using System.Collections;
using System.Collections.Generic;

namespace Elmah.SignalR.Test
{
    // Implementing an errors store, where things
    // like errors persistence and sources activation
    // are handled. This implementation draft works
    // on static member, this should be enhanced
    // to other mechanisms.

    public class ErrorsStore : IEnumerable<ErrorsSource>
    {
        private readonly Dictionary<string, ErrorsSource> _sources = new Dictionary<string, ErrorsSource>();
        private int _counter = 0;

        private ErrorsStore()
        {   
        }

        public static readonly ErrorsStore Store = new ErrorsStore();

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
    }

    public class ErrorsSource : IEnumerable<Error>
    {
        private readonly string _applicationName;
        private readonly string _handshakeToken;
        private readonly int _id;
        private readonly List<Error> _errors = new List<Error>();

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

        public ErrorsSource AppendError(Error error)
        {
            _errors.Add(error);
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
    }
}