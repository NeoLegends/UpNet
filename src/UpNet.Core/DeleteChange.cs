using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpNet.Core
{
    [DataContract]
    public class DeleteChange : Change, IEquatable<DeleteChange>
    {
        public DeleteChange(String relativePath)
            : base(int.MaxValue, relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
        }

        public override Task ApplyAsync(DataSource.IDataSource dataSource, String localPath)
        {
            return Task.FromResult(true);
        }

        public override Task FinishApplyAsync(DataSource.IDataSource dataSource, String localPath, bool updateSucceeded)
        {
            return updateSucceeded ?
                Task.Run(() => File.Delete(Path.Combine(localPath, this.RelativePath))) :
                Task.FromResult(true);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            DeleteChange change = obj as DeleteChange;
            return (obj != null) ? this.Equals(change) : false;
        }

        public bool Equals(DeleteChange other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return new { this.Priority, this.RelativePath }.GetHashCode();
        }

        public static bool operator ==(DeleteChange left, DeleteChange right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(DeleteChange left, DeleteChange right)
        {
            return !(left == right);
        }
    }
}
