using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract]
    public class AddOrReplaceChange : Change, IEquatable<AddOrReplaceChange>
    {
        [DataMember]
        public String DataSourcePath { get; private set; }

        [DataMember]
        public String Sha1 { get; private set; }

        public AddOrReplaceChange(String dataSourcePath, String relativePath, String sha1)
            : this(dataSourcePath, relativePath, sha1, 0)
        {
            Contract.Requires<ArgumentNullException>(dataSourcePath != null);
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentNullException>(sha1 != null);
        }

        public AddOrReplaceChange(String dataSourcePath, String relativePath, String sha1, int priority)
            : base(relativePath, priority)
        {
            Contract.Requires<ArgumentNullException>(dataSourcePath != null);
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentNullException>(sha1 != null);

            this.DataSourcePath = dataSourcePath;
            this.Sha1 = sha1;
        }

        public override async Task ApplyAsync(IDataSource dataSource, String localPath)
        {
            String fullLocalFilePath = Path.Combine(localPath, this.RelativePath);
            using (FileStream fs = new FileStream(fullLocalFilePath + ".update", FileMode.Create, FileAccess.ReadWrite))
            {
                using (Stream dataStream = await dataSource.GetItemAsync(this.DataSourcePath).ConfigureAwait(false))
                {
                    await dataStream.CopyToAsync(fs).ConfigureAwait(false);
                }

                fs.Position = 0;
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] computedHash = await Task.Run(() => sha1.ComputeHash(fs)).ConfigureAwait(false);
                    if (!computedHash.SequenceEqual(Convert.FromBase64String(this.Sha1)))
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                "The computed hash '{0}' didn't match the actual hash '{1}.'",
                                Convert.ToBase64String(computedHash),
                                this.Sha1
                            )
                        );
                    }
                }
            }
        }

        public override Task FinishApplyAsync(IDataSource dataSource, String localPath, bool updateSucceeded)
        {
            return Task.Run(() =>
            {
                String fullLocalFilePath = Path.Combine(localPath, this.RelativePath);
                if (updateSucceeded)
                {
                    File.Replace(fullLocalFilePath + ".update", fullLocalFilePath, null);
                }
                else
                {
                    File.Delete(fullLocalFilePath + ".update");
                }
            });
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            AddOrReplaceChange change = obj as AddOrReplaceChange;
            return (obj != null) ? this.Equals(change) : false;
        }

        public bool Equals(AddOrReplaceChange other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;

            return base.Equals(other) &&
                   (this.DataSourcePath == other.DataSourcePath) && (this.Sha1 == other.Sha1);
        }

        public override int GetHashCode()
        {
            return new { this.DataSourcePath, this.Priority, this.RelativePath, this.Sha1 }.GetHashCode();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.DataSourcePath != null);
            Contract.Invariant(this.RelativePath != null);
            Contract.Invariant(this.Sha1 != null);
        }

        public static bool operator ==(AddOrReplaceChange left, AddOrReplaceChange right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(AddOrReplaceChange left, AddOrReplaceChange right)
        {
            return !(left == right);
        }
    }
}
