#region License, Terms and Author(s)
//
// ELMAH Sandbox
// Copyright (c) 2010-11 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Pablo Cibraro
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Elmah.MongoDb
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MongoDB;
    using System.Collections.Specialized;
    using System.Collections;

    #endregion
    
    /// <summary>
    /// Converts an <see cref="Error"/> implementation into a MongoDB document.
    /// </summary>
    public static class ErrorDocument
    {
        public static Document EncodeDocument(Error error)
        {
            if (error == null) throw new ArgumentNullException("error");

            var document = new Document
            {
                { "host",               error.HostName           },
                { "type",               error.Type               },
                { "message",            error.Message            },
                { "source",             error.Source             },
                { "detail",             error.Detail             },
                { "user",               error.User               },
                { "time",               error.Time               },
                { "statusCode",         error.StatusCode         },
                { "webHostHtmlMessage", error.WebHostHtmlMessage },
            };

            SaveCollection(error.ServerVariables, document, "serverVariables");
            SaveCollection(error.QueryString, document, "queryString");
            SaveCollection(error.Form, document, "form");
            SaveCollection(error.Cookies, document, "cookies");

            return document;
        }

        private static void SaveCollection(NameValueCollection collection, Document document, string name)
        {
            if (collection.Count > 0)
            {
                var items = from key in collection.AllKeys
                            select new { name = key, value = collection[key] };
                document.Add(name, items);
            }
        }

        public static Error DecodeError(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var error = new Error
            {
                HostName            = (string)   document["host"],
                Type                = (string)   document["type"],
                Message             = (string)   document["message"],
                Source              = (string)   document["source"],
                Detail              = (string)   document["detail"],
                User                = (string)   document["user"],
                Time                = (DateTime) document["time"],
                StatusCode          = (int)      document["statusCode"],
                WebHostHtmlMessage  = (string)   document["webHostHtmlMessage"]
            };

            AddDocumentItemsToCollection(document["serverVariables"], error.ServerVariables);
            AddDocumentItemsToCollection(document["queryString"], error.QueryString);
            AddDocumentItemsToCollection(document["form"], error.Form);
            AddDocumentItemsToCollection(document["cookies"], error.Cookies);

            return error;
        }

        private static void AddDocumentItemsToCollection(object documents, NameValueCollection collection)
        {
            AddDocumentItemsToCollection((IEnumerable<Document>) documents, collection);
        }

        private static void AddDocumentItemsToCollection(IEnumerable<Document> documents, NameValueCollection collection)
        {
            if (documents == null) 
                return;

            foreach (var document in documents)
            {
                var key = (string) document["name"];
                var value = (string) document["value"];
                collection.Add(key, value);
            }
        }
    }
}
