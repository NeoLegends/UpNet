using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UpNet.Core.DataSource
{
    public class HttpDataSource : IDataSource
    {
        private Uri serverUri;

        private String updateFileName;

        public HttpDataSource(String uri, String updateFileName)
            : this(new Uri(uri, UriKind.Absolute), updateFileName)
        {
            Contract.Requires<ArgumentNullException>(uri != null);
            Contract.Requires<ArgumentNullException>(updateFileName != null);
        }

        public HttpDataSource(Uri uri, String updateFileName)
        {
            Contract.Requires<ArgumentNullException>(uri != null);
            Contract.Requires<ArgumentNullException>(updateFileName != null);

            this.serverUri = uri;
            this.updateFileName = updateFileName;
        }

        public Task<Stream> GetItemAsync(String dataSourcePath)
        {
            using (HttpClient client = new HttpClient())
            {
                return client.GetStreamAsync(new Uri(this.serverUri, dataSourcePath));
            }
        }

        public async Task<Update> GetUpdateAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                String json = await client.GetStringAsync(new Uri(this.serverUri, this.updateFileName));
                return await Task.Run(() => JsonConvert.DeserializeObject<Update>(json));
            }
        }
    }
}
