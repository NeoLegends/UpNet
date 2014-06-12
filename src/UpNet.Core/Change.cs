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
    public abstract class Change : IEquatable<Change>
    {
        [DataMember]
        public int Priority { get; protected set; }

        [DataMember]
        public String RelativePath { get; protected set; }

        protected Change() { }

        protected Change(String relativePath, int priority)
        {
            this.Priority = priority;
            this.RelativePath = relativePath;
        }

        public abstract Task ApplyAsync(IDataSource dataSource, String localPath);

        public abstract Task FinishApplyAsync(IDataSource dataSource, String localPath, bool updateSucceeded);

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
            return new { this.Priority, this.RelativePath }.GetHashCode();
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
