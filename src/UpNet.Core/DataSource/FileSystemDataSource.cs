using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core.DataSource
{
    public class FileSystemDataSource : IDataSource
    {
        public String LocalPath { get; private set; }

        public String UpdateFilePath { get; private set; }

        public FileSystemDataSource(String localPath, String updateFilePath)
        {
            Contract.Requires<ArgumentNullException>(localPath != null);
            Contract.Requires<ArgumentNullException>(updateFilePath != null);

            this.LocalPath = localPath;
            this.UpdateFilePath = updateFilePath;
        }

        async Task<Stream> IDataSource.GetItemAsync(String dataSourcePath)
        {
            return await this.GetItemAsync(dataSourcePath);
        }

        public Task<FileStream> GetItemAsync(String dataSourcePath)
        {
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
