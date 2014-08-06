using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract]
    public class Patch : IEnumerable<Change>, IEquatable<Patch>
    {
        [DataMember]
        public ImmutableList<Change> Changes { get; private set; }

        [DataMember]
        public UserMeta Meta { get; private set; }

        [DataMember]
        public Version Version { get; private set; }

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

            return Task.WhenAll(this.Changes.OrderByDescending(change => change.Priority)
                                            .Select(change => change.ApplyAsync(dataSource, localPath))
            );
        }

        public Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<InvalidOperationException>(this.Changes != null);

            return Task.WhenAll(this.Changes.OrderByDescending(change => change.Priority)
                                            .Select(change => change.FinishApplyAsync(dataSource, localPath, updateSucceeded))
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

            return (this.Count == other.Count) && (this.Changes.SequenceEqual(other.Changes)) && 
                   (this.Meta == other.Meta) && (this.Version == other.Version);
        }

        public IEnumerator<Change> GetEnumerator()
        {
            IEnumerable<Change> changes = this.Changes;
            return (changes != null) ? changes.GetEnumerator() : null;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return new { ChangeCount = this.Count, this.Changes, this.Meta, this.Version }.GetHashCode();
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
