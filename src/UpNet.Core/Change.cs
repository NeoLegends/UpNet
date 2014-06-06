using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract]
    [KnownType(typeof(AddOrReplaceChange)), KnownType(typeof(DeleteChange))]
    [ContractClass(typeof(ChangeContracts))]
    public abstract class Change
    {
        [IgnoreDataMember]
        public int Priority { get; protected set; }

        [DataMember]
        public String RelativePath { get; protected set; }

        public Change() { }

        public Change(int priority, String relativePath)
        {
            this.Priority = priority;
            this.RelativePath = relativePath;
        }

        public abstract Task ApplyAsync(IDataSource dataSource, String localPath);

        public abstract Task FinishApplyAsync(IDataSource dataSource, String localPath, bool updateSucceeded);
    }

    [ContractClassFor(typeof(Change))]
    abstract class ChangeContracts : Change
    {
        public override Task ApplyAsync(IDataSource dataSource, String localPath)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(localPath != null);

            return null;
        }

        public override Task FinishApplyAsync(IDataSource dataSource, String localPath, bool updateSucceeded)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(localPath != null);

            return null;
        }
    }
}
