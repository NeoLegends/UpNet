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
        public event ProgressChangedEventHandler UpdateProgressChanged;

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

            int changeCount = Math.Max(this.Changes.Count(), 1);
            int finishedTasks = 0;
            return Task.WhenAll(this.Changes.Select(change => change.Apply(dataSource, localPath).ContinueWith(t =>
            {
                this.RaiseProgessChanged(Interlocked.Increment(ref finishedTasks) / changeCount, change.RelativePath);
            })));
        }

        private void RaiseProgessChanged(int percentage, object state = null)
        {
            ProgressChangedEventHandler handler = this.UpdateProgressChanged;
            if (handler != null)
            {
                handler(this, new ProgressChangedEventArgs(percentage, state));
            }
        }
    }
}
