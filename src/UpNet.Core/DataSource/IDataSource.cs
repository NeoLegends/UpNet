using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core.DataSource
{
    public interface IDataSource
    {
        Task<Stream> GetItemAsync(String dataSourcePath);

        Task<Update> GetUpdateAsync();
    }
}
