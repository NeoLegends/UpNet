using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core.DataSource
{
    public class HttpDataSource : IDataSource
    {
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

        public Task<Stream> GetItemAsync(string dataSourcePath)
        {
            using (HttpClient client = new HttpClient())
            {
                return client.GetStreamAsync(new Uri(this.ServerUri, dataSourcePath));
            }
        }

        public async Task<Update> GetUpdateAsync()
        {
            using (HttpClient client = new HttpClient())
            using (Stream updateStream = await client.GetStreamAsync(new Uri(this.ServerUri, this.UpdateFileName)).ConfigureAwait(false))
            {
                return await Task.Run(() =>
                {
                    Update update = (Update)new DataContractSerializer(typeof(Update)).ReadObject(updateStream);
                    update.DataSource = this;
                    return update;
                }).ConfigureAwait(false);
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
