using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core
{
    [DataContract]
    public struct Patch
    {
        [DataMember]
        public IEnumerable<Change> ChangedFiles { get; private set; }

        [DataMember]
        public UserMeta Meta { get; private set; }

        [DataMember]
        public Version Version { get; private set; }

        public Patch(IEnumerable<Change> changedFiles, UserMeta meta, Version version)
            : this()
        {
            Contract.Requires<ArgumentNullException>(changedFiles != null);
            Contract.Requires<ArgumentNullException>(version != null);

            this.ChangedFiles = changedFiles.ToImmutableList();
            this.Meta = meta;
            this.Version = version;
        }
    }
}
