using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpNet.Core.DataSource
{
    [ContractClass(typeof(IDataSourceContracts))]
    public interface IDataSource
    {
        Task<Stream> GetItemAsync(string dataSourcePath, CancellationToken token);

        Task<Update> GetUpdateAsync(CancellationToken token);
    }

    [ContractClassFor(typeof(IDataSource))]
    abstract class IDataSourceContracts : IDataSource
    {
        Task<Stream> IDataSource.GetItemAsync(string dataSourcePath, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(dataSourcePath));

            return null;
        }

        Task<Update> IDataSource.GetUpdateAsync(CancellationToken token)
        {
            return null;
        }
    }
}
