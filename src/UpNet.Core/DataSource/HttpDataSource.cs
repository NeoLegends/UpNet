using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UpNet.Core.DataSource
{
    public class HttpDataSource : IDataSource
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public Uri ServerUri { get; private set; }

        public string UpdateFileName { get; private set; }

        public HttpDataSource(string uri, string updateFileName)
            : this(new Uri(uri, UriKind.Absolute), updateFileName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(uri));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(updateFileName));
        }

        public HttpDataSource(Uri uri, string updateFileName)
        {
            Contract.Requires<ArgumentNullException>(uri != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(updateFileName));

            this.ServerUri = uri;
            this.UpdateFileName = updateFileName;
        }

        public Task<Stream> GetItemAsync(string dataSourcePath, CancellationToken token)
        {
            return httpClient.GetStreamAsync(new Uri(this.ServerUri, dataSourcePath));
        }

        public async Task<Update> GetUpdateAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (Stream s = await httpClient.GetStreamAsync(new Uri(this.ServerUri, this.UpdateFileName)).ConfigureAwait(false))
            using (StreamReader sr = new StreamReader(s))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                return (new JsonSerializer() { Context = new StreamingContext(StreamingContextStates.All, this), NullValueHandling = NullValueHandling.Ignore })
                           .Deserialize<Update>(jtr);
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.ServerUri != null);
            Contract.Invariant(this.UpdateFileName != null);
        }
    }
}
