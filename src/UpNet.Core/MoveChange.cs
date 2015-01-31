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
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract, JsonObject]
    public class MoveChange : Change
    {
        [DataMember, JsonProperty]
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

        public override Task ApplyAsync(IDataSource dataSource, string localPath, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        public override Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return updateSucceeded ? 
                Task.Run(() => File.Move(this.RelativePath, this.NewRelativePath)) :
                Task.FromResult(false);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 486187739 + base.GetHashCode();
                hash = hash * 486187739 + this.NewRelativePath.GetHashCode();
                return hash;
            }
        } 

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.NewRelativePath != null);
        }
    }
}
