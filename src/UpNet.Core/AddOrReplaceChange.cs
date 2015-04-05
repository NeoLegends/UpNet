using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract, JsonObject]
    public class AddOrReplaceChange : Change, IEquatable<AddOrReplaceChange>
    {
        [DataMember, JsonProperty]
        public string DataSourcePath { get; private set; }

        [DataMember, JsonProperty]
        public string Sha256 { get; private set; }

        public AddOrReplaceChange(string dataSourcePath, string relativePath, string sha1)
            : this(dataSourcePath, relativePath, sha1, 0)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(dataSourcePath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(relativePath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sha1));
        }

        public AddOrReplaceChange(string dataSourcePath, string relativePath, string sha1, int priority)
            : base(relativePath, priority)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(dataSourcePath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(relativePath));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sha1));

            this.DataSourcePath = dataSourcePath;
            this.Sha256 = sha1;
        }

        public override async Task ApplyAsync(IDataSource dataSource, string localPath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string fullLocalFilePath = Path.Combine(localPath, this.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullLocalFilePath));

            using (FileStream fs = new FileStream(fullLocalFilePath + ".update", FileMode.Create, FileAccess.ReadWrite))
            {
                using (Stream dataStream = await dataSource.GetItemAsync(this.DataSourcePath, token).ConfigureAwait(false))
                {
                    await dataStream.CopyToAsync(fs, 4096, token).ConfigureAwait(false);
                }

                await fs.FlushAsync(token).ConfigureAwait(false);
                fs.Position = 0;
                using (SHA256 sha1 = SHA256.Create())
                {
                    byte[] computedHash = await Task.Run(() => sha1.ComputeHash(fs)).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (!computedHash.SequenceEqual(Convert.FromBase64String(this.Sha256)))
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "The computed SHA256 hash '{0}' didn't match the actual hash '{1}.'",
                                Convert.ToBase64String(computedHash),
                                this.Sha256
                            )
                        );
                    }
                }
            }
        }

        public override Task FinishApplyAsync(IDataSource dataSource, string localPath, bool updateSucceeded, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                string fullLocalFilePath = Path.Combine(localPath, this.RelativePath);
                if (updateSucceeded)
                {
                    File.Replace(fullLocalFilePath + ".update", fullLocalFilePath, null);
                }
                else
                {
                    File.Delete(fullLocalFilePath + ".update");
                }
            }, token);
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
                   (this.DataSourcePath == other.DataSourcePath) && (this.Sha256 == other.Sha256);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 486187739 + base.GetHashCode();
                hash = hash * 486187739 + this.DataSourcePath.GetHashCode();
                hash = hash * 486187739 + this.Sha256.GetHashCode();
                return hash;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.DataSourcePath != null);
            Contract.Invariant(this.RelativePath != null);
            Contract.Invariant(this.Sha256 != null);
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
