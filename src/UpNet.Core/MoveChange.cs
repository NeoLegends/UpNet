using System;
using System.Collections.Generic;
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
    public class MoveChange : Change
    {
        [DataMember]
        public string NewRelativePath { get; private set; }

        public MoveChange(string oldPath, string newPath)
            : this(oldPath, newPath, int.MaxValue / 2)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(oldPath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(newPath));
        }

        public MoveChange(string oldPath, string newPath, int priority)
            : base(oldPath, priority)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(oldPath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(newPath));
        }

        public override Task ApplyAsync(IDataSource dataSource, string localPath)
        {
            return Task.FromResult(true);
        }

        public override Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded)
        {
            return updateSucceeded ? 
                Task.Run(() => File.Move(this.RelativePath, this.NewRelativePath)) :
                Task.FromResult(false);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.NewRelativePath != null);
        }
    }
}
