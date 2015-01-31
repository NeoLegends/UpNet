using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
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
    public class Patch : IEnumerable<Change>, IEquatable<Patch>
    {
        [DataMember, JsonProperty]
        public ImmutableList<Change> Changes { get; private set; }

        [DataMember, JsonProperty]
        public UserMeta Meta { get; private set; }

        [DataMember, JsonProperty]
        public Version Version { get; private set; }

        [IgnoreDataMember, JsonIgnore]
        public int Count
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

        public Task ApplyAsync(IDataSource dataSource, string localPath)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return this.ApplyAsync(dataSource, localPath, CancellationToken.None);
        }

        public Task ApplyAsync(IDataSource dataSource, string localPath, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return Task.WhenAll(this.Changes.OrderByDescending(change => change.Priority)
                                            .Select(change => change.ApplyAsync(dataSource, localPath, token))
            );
        }

        public Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return this.FinishApplyAsync(dataSource, localPath, updateSucceeded, CancellationToken.None);
        }

        public Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return Task.WhenAll(this.Changes.OrderByDescending(change => change.Priority)
                                            .Select(change => change.FinishApplyAsync(dataSource, localPath, updateSucceeded, token))
            );
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            Patch patch = obj as Patch;
            return (patch != null) ? this.Equals(patch) : false;
        }

        public bool Equals(Patch other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;

            return (this.Version == other.Version) && (this.Meta == other.Meta) && (this.Changes.SequenceEqual(other.Changes));
        }

        public IEnumerator<Change> GetEnumerator()
        {
            return this.Changes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 486187739 + this.Changes.GetHashCode();
                hash = hash * 486187739 + this.Meta.GetHashCode();
                hash = hash * 486187739 + this.Version.GetHashCode();
                return hash;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.Changes != null);
        }

        public static bool operator ==(Patch left, Patch right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Patch left, Patch right)
        {
            return !(left == right);
        }
    }
}
