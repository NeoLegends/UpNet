using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpNet.Core;

namespace UpNet.Core.DataSource
{
    public class FileSystemDataSource : IDataSource
    {
        private static readonly JsonSerializer serializer = JsonSerializer.CreateDefault();

        public string LocalPath { get; private set; }

        public string UpdateFilePath { get; private set; }

        public FileSystemDataSource(string localPath, string updateFilePath)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(updateFilePath));

            this.LocalPath = localPath;
            this.UpdateFilePath = updateFilePath;
        }

        async Task<Stream> IDataSource.GetItemAsync(string dataSourcePath, CancellationToken token)
        {
            return await this.GetItemAsync(dataSourcePath, token);
        }

        public Task<FileStream> GetItemAsync(string dataSourcePath, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(dataSourcePath));

            token.ThrowIfCancellationRequested();
            return Task.FromResult(File.OpenRead(Path.Combine(this.LocalPath, dataSourcePath)));
        }

        public Task<Update> GetUpdateAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                using (FileStream fs = File.OpenRead(Path.Combine(this.LocalPath, this.UpdateFilePath)))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonTextReader jtr = new JsonTextReader(sr))
                {
                    token.ThrowIfCancellationRequested();

                    Update update = serializer.Deserialize<Update>(jtr);
                    update.DataSource = this;
                    return update;
                }
            }, token);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.LocalPath != null);
            Contract.Invariant(this.UpdateFilePath != null);
        }
    }
}
