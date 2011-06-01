namespace Elmah.MongoDb
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using MongoDB;
    using System.Collections.Specialized;
    using System.Collections;

    #endregion
    
    /// <summary>
	/// Converts an <see cref="ErrorLog"/> implementation into a MongoDB document
	/// </summary>
	public class ErrorDocument
	{
		public static Document EncodeDocument(Error error)
		{
			var document = new Document();
			document.Add("host", error.HostName);
			document.Add("type", error.Type);
			document.Add("message", error.Message);
			document.Add("source", error.Source);
			document.Add("detail", error.Detail);
			document.Add("user", error.User);
			document.Add("time", error.Time);
			document.Add("statusCode", error.StatusCode);
			document.Add("webHostHtmlMessage", error.WebHostHtmlMessage);

			if (error.ServerVariables.Count > 0)
			{
				document.Add("serverVariables", GetCollection(error.ServerVariables));
			}
			
			if(error.QueryString.Count > 0)
				document.Add("queryString", GetCollection(error.QueryString));
			
			if(error.Form.Count > 0)
				document.Add("form", GetCollection(error.Form));
			
			if(error.Cookies.Count > 0)
				document.Add("cookies", GetCollection(error.Cookies));

			return document;
		}

		public static Error DecodeError(Document document)
		{
			var error = new Error();
			
			error.HostName = (string)document["host"];
			error.Type = (string)document["type"];
			error.Message = (string)document["message"];
			error.Source = (string)document["source"];
			error.Detail = (string)document["detail"];
			error.User = (string)document["user"];
			error.Time = (DateTime)document["time"];
			error.StatusCode = (int)document["statusCode"];
			error.WebHostHtmlMessage = (string)document["webHostHtmlMessage"];

			AddDocumentItemsToCollection((List<Document>)document["serverVariables"], error.ServerVariables);
			AddDocumentItemsToCollection((List<Document>)document["queryString"], error.QueryString);
			AddDocumentItemsToCollection((List<Document>)document["form"], error.Form);
			AddDocumentItemsToCollection((List<Document>)document["cookies"], error.Cookies);

			return error;
		}

		private static IEnumerable GetCollection(NameValueCollection collection)
		{
			foreach (var key in collection.AllKeys)
			{
				yield return new { name = key, value = collection[key] };
			}
		}

		private static void AddDocumentItemsToCollection(List<Document> documents, NameValueCollection collection)
		{
			if (documents != null)
			{
				foreach (var document in documents)
				{
					var key = (string)document["name"];
					var value = (string)document["value"];

					collection.Add(key, value);
				}
			}
		}
	}
}
