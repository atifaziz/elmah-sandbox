#region License, Terms and Author(s)
//
// ELMAH Sandbox
// Copyright (c) 2014 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
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

namespace Elmah.WebArchiver
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    #endregion

    public class ErrorLogArchiveHandler : HttpTaskAsyncHandler
    {
        public async override Task ProcessRequestAsync(HttpContext context)
        {
            var log = GetErrorLog(context);
            var response = context.Response;
            response.BufferOutput = false;
            response.ContentType = "application/zip";
            response.Headers["Content-Disposition"] = "attachement; filename=errorlog.zip";

            using (var zip = new ZipArchive(new PositionTrackingOutputStream(response.OutputStream), ZipArchiveMode.Create, leaveOpen: true))
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(context.Request.TimedOutToken, context.Response.ClientDisconnectedToken))
            {
                // ReSharper disable once AccessToDisposedClosure
                await Archive(log, Encoding.UTF8, e => zip.CreateEntry(string.Format("error-{0}.xml", e.Id)).Open(), cts.Token);
            }
        }

        protected virtual ErrorLog GetErrorLog(HttpContext context)
        {
            return ErrorLog.GetDefault(context);
        }

        static Task Archive(ErrorLog log, Encoding encoding, Func<ErrorLogEntry, Stream> opener, CancellationToken cancellationToken)
        {
            return Archive(log.GetErrorsAsync, log.GetErrorAsync, encoding, opener, cancellationToken);
        }

        static async Task Archive(
            Func<int, int, ICollection<ErrorLogEntry>, CancellationToken, Task<int>> pager,
            Func<string, CancellationToken, Task<ErrorLogEntry>> detailer, 
            Encoding encoding, Func<ErrorLogEntry, Stream> opener, 
            CancellationToken cancellationToken)
        {
            if (pager == null) throw new ArgumentNullException("pager");
            if (detailer == null) throw new ArgumentNullException("detailer");
            if (encoding == null) throw new ArgumentNullException("encoding");
            if (opener == null) throw new ArgumentNullException("opener");

            for (var pageIndex = 0; ; pageIndex++)
            {
                const int pageSize = 100;
                var entries = new List<ErrorLogEntry>(pageSize);
                cancellationToken.ThrowIfCancellationRequested();
                await pager(pageIndex, pageSize, entries, cancellationToken);

                if (entries.Count == 0)
                    break;

                foreach (var e in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var detail = await detailer(e.Id, cancellationToken);
                    using (var entryStream = opener(e))
                    {
                        var bytes = encoding.GetBytes(ErrorXml.EncodeString(detail.Error));
                        await entryStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                    }
                }
            }
        }
    }
}
