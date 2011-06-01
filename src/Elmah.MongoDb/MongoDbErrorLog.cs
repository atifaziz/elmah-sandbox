namespace Elmah.MongoDb
{
    #region Imports

    using System;
    using System.Linq;
    using System.Threading;
    using MongoDB;
    using System.Configuration;
    using System.Collections;

    using IDictionary = System.Collections.IDictionary;

    #endregion

    /// <summary>
    /// An <see cref="ErrorLog"/> implementation that uses MongoDb 
    /// as its backing store.
    /// </summary>
    public class MongoDbErrorLog : ErrorLog
    {
        private readonly string _connectionString;

        private const int _maxAppNameLength = 60;
        private const int _maxEntriesCount = 10000; // Default max 10000 entries 
        private const int _defaultCollectionSize = 20 * 1024 * 1024; // 20 MB default collection size

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlErrorLog"/> class
        /// using a dictionary of configured settings.
        /// </summary>
        public MongoDbErrorLog(IDictionary config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            var connectionString = GetConnectionString(config);

            //
            // If there is no connection string to use then throw an 
            // exception to abort construction.
            //

            if (connectionString.Length == 0)
                throw new ApplicationException("Connection string is missing for the SQL error log.");

            _connectionString = connectionString;

            //
            // Set the application name as this implementation provides
            // per-application isolation over a single store.
            //
            var appName = String.Empty;
            if (config["applicationName"] != null)
                appName = (string)config["applicationName"];

            if (appName.Length > _maxAppNameLength)
            {
                throw new ApplicationException(string.Format(
                    "Application name is too long. Maximum length allowed is {0} characters.",
                    _maxAppNameLength.ToString("N0")));
            }

            ApplicationName = appName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbErrorLog"/> class
        /// to use a specific connection string for connecting to the database.
        /// </summary>
        public MongoDbErrorLog(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            if (connectionString.Length == 0)
                throw new ArgumentException(null, "connectionString");

            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>
        public override string Name
        {
            get { return "MongoDb Error Log"; }
        }

        /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Logs an error to the database.
        /// </summary>
        /// <remarks>
        /// Use the stored procedure called by this implementation to set a
        /// policy on how long errors are kept in the log. The default
        /// implementation stores all errors for an indefinite time.
        /// </remarks>

        public override string Log(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            var id = Guid.NewGuid().ToString();

            var document = ErrorDocument.EncodeDocument(error);
            document.Add("id", id);

            using (var mongo = new Mongo(_connectionString))
            {
                mongo.Connect();

                var master = mongo.GetDatabase("master");

                IMongoCollection collection = null;

                if (!master.GetCollectionNames().Any(collectionName => collectionName.EndsWith(ApplicationName)))
                {
                    // Create event collection
                    var options = new Document();
                    options.Add("capped", true);
                    options.Add("max", _maxEntriesCount);
                    options.Add("size", _defaultCollectionSize);
                    
                    master.Metadata.CreateCollection(ApplicationName, options);
                
                    var indexes = new Document();
                    indexes.Add("id", 1);

                    collection = master.GetCollection(ApplicationName);
                    collection.MetaData.Indexes.Add("id", indexes);
                }

                if(collection == null)
                    collection = master.GetCollection(ApplicationName);
                
                collection.Save(document);
            }

            return id;
        }

        /// <summary>
        /// Returns a page of errors from the databse in descending order 
        /// of logged time.
        /// </summary>

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);
            if (pageSize < 0) throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            using (var mongo = new Mongo())
            {
                mongo.Connect();

                var master = mongo.GetDatabase("master");
                
                var collection = master.GetCollection(ApplicationName);
                var documents = collection.FindAll()
                    .Skip(pageIndex * pageSize)
                    .Limit(pageSize)
                    .Documents;

                foreach (var document in documents)
                {
                    var errorLog = ErrorDocument.DecodeError(document);
                    errorEntryList.Add(new ErrorLogEntry(this, (string)document["id"], errorLog));
                }

                return collection.FindAll().Documents.Count();
            }
        }

        /// <summary>
        /// Begins an asynchronous version of <see cref="GetErrors"/>.
        /// </summary>
        public override IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList,
            AsyncCallback asyncCallback, object asyncState)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);
            if (pageSize < 0) throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            //The MongoDb driver implementation does not support async calls
            //so the sync version is used
            var count = GetErrors(pageIndex, pageSize, errorEntryList);

            var asyncResult = new AsyncResultWrapper(errorEntryList, count);
            
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous version of <see cref="ErrorLog.GetErrors"/>.
        /// </summary>
        public override int EndGetErrors(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            var wrapper = asyncResult as AsyncResultWrapper;

            if (wrapper == null)
                throw new ArgumentException("Unexepcted IAsyncResult type.", "asyncResult");

            return wrapper.Count;
        }

        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>

        public override ErrorLogEntry GetError(string id)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (id.Length == 0) throw new ArgumentException(null, "id");

            using (var mongo = new Mongo())
            {
                mongo.Connect();

                var master = mongo.GetDatabase("master");

                var searchDocument = new Document();
                searchDocument.Add("id", id);

                var collection = master.GetCollection(ApplicationName);
                var document = collection.FindOne(searchDocument);

                if (document == null)
                {
                    return null;
                }

                var errorLog = ErrorDocument.DecodeError(document);

                return new ErrorLogEntry(this, id, errorLog);
            }
        }

        private static string GetConnectionString(IDictionary config)
        {
            //
            // First look for a connection string name that can be 
            // subsequently indexed into the <connectionStrings> section of 
            // the configuration to get the actual connection string.
            //

            string connectionStringName = (string)config["connectionStringName"];

            if (!string.IsNullOrEmpty(connectionStringName))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];

                if (settings == null)
                    return string.Empty;

                return settings.ConnectionString ?? string.Empty;
            }

            //
            // Connection string name not found so see if a connection 
            // string was given directly.
            //

            var connectionString = (string)config["connectionString"];
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;

            //
            // As a last resort, check for another setting called 
            // connectionStringAppKey. The specifies the key in 
            // <appSettings> that contains the actual connection string to 
            // be used.
            //

            var connectionStringAppKey = (string)config["connectionStringAppKey"];
            return !string.IsNullOrEmpty(connectionStringAppKey)
                 ? ConfigurationManager.AppSettings[connectionStringAppKey]
                 : string.Empty;
        }

        /// <summary>
        /// An <see cref="IAsyncResult"/> implementation that wraps another.
        /// </summary>

        private sealed class AsyncResultWrapper : IAsyncResult
        {
            public AsyncResultWrapper(IList entries, int count)
            {
                this.Count = count;
                this.Entries = entries;
            }

            public int Count { get; private set; }

            public IList Entries { get; private set; }

            public bool IsCompleted
            {
                get { return true; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotSupportedException("Not supported"); }
            }

            public object AsyncState { get; private set; }

            public bool CompletedSynchronously
            {
                get { return true; }
            }
        }
    }
}
