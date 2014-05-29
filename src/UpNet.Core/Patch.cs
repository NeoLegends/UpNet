using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract]
    public class Patch
    {
        [DataMember]
        public IEnumerable<Change> Changes { get; private set; }

        [DataMember]
        public UserMeta Meta { get; private set; }

        [DataMember]
        public Version Version { get; private set; }

        public int ChangeCount
        {
            get
            {
                IEnumerable<Change> changes = this.Changes;
                return (changes != null) ? changes.Count() : 0;
            }
        }

        private Patch() { }

        public Patch(IEnumerable<Change> changes, UserMeta meta, Version version)
        {
            Contract.Requires<ArgumentNullException>(changes != null);
            Contract.Requires<ArgumentNullException>(version != null);

            this.Changes = changes.ToImmutableList();
            this.Meta = meta;
            this.Version = version;
        }

        public Task Apply(IDataSource dataSource, String localPath)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(localPath != null);
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return Task.WhenAll(this.Changes.OrderByDescending(change => change.Action)
                                            .Select(change => change.Apply(dataSource, localPath))
            );
        }

        public Task FinishApply(IDataSource dataSource, String localPath, bool updateSucceeded)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(localPath != null);
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return Task.WhenAll(this.Changes.OrderByDescending(change => change.Action)
                                            .Select(change => change.FinishApply(dataSource, localPath, updateSucceeded))
            );
        }
    }
}
