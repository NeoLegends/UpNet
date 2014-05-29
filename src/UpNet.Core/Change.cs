using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract]
    public struct Change
    {
        [DataMember]
        public FileAction Action { get; private set; }

        [DataMember]
        public String DataSourcePath { get; private set; }

        [DataMember]
        public String RelativePath { get; private set; }

        [DataMember]
        public String Sha1 { get; private set; }

        public Change(String localPath)
            : this(FileAction.Delete, null, localPath, null)
        {
            Contract.Requires<ArgumentNullException>(localPath != null);
        }

        public Change(FileAction action, String dataSourcePath, String relativePath, String sha1)
            : this()
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentNullException>(action == FileAction.Delete || dataSourcePath != null); // If we're deleting, we don't need the
            Contract.Requires<ArgumentNullException>(action == FileAction.Delete || sha1 != null);           // data source key or hash.

            this.Action = action;
            this.DataSourcePath = dataSourcePath;
            this.RelativePath = relativePath;
            this.Sha1 = sha1;
        }

        public async Task Apply(IDataSource dataSource, String localPath)
        {
            Contract.Requires<ArgumentNullException>(this.Action == FileAction.Delete || dataSource != null);
            Contract.Requires<ArgumentNullException>(localPath != null);

            String fullPath = Path.Combine(localPath, this.RelativePath);
            switch (this.Action)
            {
                case FileAction.AddOrReplace:
                    using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    using (Stream dataStream = await dataSource.GetItemAsync(this.DataSourcePath))
                    {
                        await dataStream.CopyToAsync(fs);
                    }
                    break;
                case FileAction.Delete:
                    await Task.Run(() => File.Delete(fullPath));
                    break;
            }
        }
    }
}
