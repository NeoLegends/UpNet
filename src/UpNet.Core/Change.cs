using System;
using System.Collections.Generic;
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
    [ContractClass(typeof(ChangeContracts))]
    [KnownType(typeof(AddOrReplaceChange)), KnownType(typeof(DeleteChange)), KnownType(typeof(MoveChange))]
    public abstract class Change : IEquatable<Change>
    {
        [DataMember, JsonProperty]
        public int Priority { get; protected set; }

        [DataMember, JsonProperty]
        public string RelativePath { get; protected set; }

        protected Change() { }

        protected Change(string relativePath, int priority)
        {
            this.Priority = priority;
            this.RelativePath = relativePath;
        }

        public Task ApplyAsync(IDataSource dataSource, string localPath)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));

            return this.ApplyAsync(dataSource, localPath, CancellationToken.None);
        }

        public abstract Task ApplyAsync(IDataSource dataSource, string localPath, CancellationToken token);

        public Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));

            return this.FinishApplyAsync(dataSource, localPath, updateSucceeded, CancellationToken.None);
        }

        public abstract Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded, CancellationToken token);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            Change change = obj as Change;
            return (change != null) ? this.Equals(change) : false;
        }

        public bool Equals(Change other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;

            return (this.Priority == other.Priority) && (this.RelativePath == other.RelativePath);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 486187739 + this.Priority.GetHashCode();
                hash = hash * 486187739 + this.RelativePath.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Change left, Change right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Change left, Change right)
        {
            return !(left == right);
        }
    }

    [ContractClassFor(typeof(Change))]
    abstract class ChangeContracts : Change
    {
        public override Task ApplyAsync(IDataSource dataSource, string localPath, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));

            return null;
        }

        public override Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));

            return null;
        }
    }
}
