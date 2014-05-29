using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core
{
    public class UpdateMeta
    {
        public String ChangeLog { get; private set; }

        public Version Version { get; private set; }

        public ChangedFile[] ChangedFiles { get; private set; }

        public struct ChangedFile
        {
            public String DownloadPath { get; private set; }

            public String LocalPath { get; private set; }

            public ChangedFile(String downloadPath, String localPath)
                : this()
            {
                this.DownloadPath = downloadPath;
                this.LocalPath = localPath;
            }
        }
    }
}
