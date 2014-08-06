using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpNet.Core;

namespace UpNet.Core.DataSource
{
    public class FileSystemDataSource : IDataSource
    {
        public string LocalPath { get; private set; }

        public string UpdateFilePath { get; private set; }

        public FileSystemDataSource(string localPath, string updateFilePath)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(updateFilePath));

            this.LocalPath = localPath;
            this.UpdateFilePath = updateFilePath;
        }

        async Task<Stream> IDataSource.GetItemAsync(string dataSourcePath)
        {
            return await this.GetItemAsync(dataSourcePath).ConfigureAwait(false);
        }

        public Task<FileStream> GetItemAsync(string dataSourcePath)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(dataSourcePath));

            return Task.FromResult(File.OpenRead(Path.Combine(this.LocalPath, dataSourcePath)));
        }

        public Task<Update> GetUpdateAsync()
        {
            return Task.Run(() =>
            {
                using (FileStream fs = File.OpenRead(Path.Combine(this.LocalPath, this.UpdateFilePath)))
                {
                    Update update = (Update)new DataContractSerializer(typeof(Update)).ReadObject(fs);
                    update.DataSource = this;
                    return update;
                }
            });
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.LocalPath != null);
            Contract.Invariant(this.UpdateFilePath != null);
        }
    }
}
