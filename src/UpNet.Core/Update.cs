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
using Newtonsoft.Json;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract, JsonObject]
    public class Update : IEnumerable<Patch>, IEquatable<Update>
    {
        public event ProgressChangedEventHandler PatchProgressChanged;

        private IDataSource _DataSource;

        [IgnoreDataMember, JsonIgnore]
        public IDataSource DataSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                if (value != null)
                {
                    if (Interlocked.CompareExchange(ref _DataSource, value, null) != null)
                    {
                        throw new InvalidOperationException("Once it has been set, DataSource cannot be set again.");
                    }
                }
            }
        }

        [DataMember, JsonProperty]
        public ImmutableList<Patch> Patches { get; private set; }

        [IgnoreDataMember, JsonIgnore]
        public int Count
        {
            get
            {
                IEnumerable<Patch> patches = this.Patches;
                return (patches != null) ? patches.Count() : 0;
            }
        }

        public Version LatestVersion
        {
            get
            {
                IEnumerable<Patch> patches = this.Patches;
                return (patches != null) ? patches.Max(patch => patch.Version) : null;
            }
        }

        private Update() : this(Enumerable.Empty<Patch>()) { }

        public Update(IEnumerable<Patch> patches)
            : this(patches, null)
        {
            Contract.Requires<ArgumentNullException>(patches != null);
        }

        public Update(IEnumerable<Patch> patches, IDataSource dataSource)
        {
            Contract.Requires<ArgumentNullException>(patches != null);

            if (dataSource != null)
            {
                this.DataSource = dataSource;
            }
            this.Patches = patches.ToImmutableList();
        }

        public Task ApplyAsync(string localPath, Version currentVersion)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<ArgumentNullException>(currentVersion != null);
            Contract.Requires<InvalidOperationException>(this.DataSource != null);

            return this.ApplyAsync(localPath, currentVersion, CancellationToken.None);
        }

        public async Task ApplyAsync(string localPath, Version currentVersion, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPath));
            Contract.Requires<ArgumentNullException>(currentVersion != null);
            Contract.Requires<InvalidOperationException>(this.DataSource != null);

            token.ThrowIfCancellationRequested();
            IEnumerable<Patch> patchesToApply = this.Patches.Where(patch => patch.Version > currentVersion)
                                                            .OrderBy(patch => patch.Version);
            bool updateSuccess = true;

            int totalPatchCount = Math.Max(patchesToApply.Count(), 1) * 2; // Times two because of two-stage updating
            int currentPatchCount = 0;

            token.ThrowIfCancellationRequested();
            List<Exception> exceptions = new List<Exception>();
            try
            {
                foreach (Patch patch in patchesToApply) // Can't be Task.WhenAll because we need to preserve the order of the patches
                {
                    await patch.ApplyAsync(this.DataSource, localPath, token);
                    this.RaisePatchProgessChanged(++currentPatchCount / totalPatchCount, patch.Version);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                updateSuccess = false;
            }

            foreach (Patch patch in patchesToApply)
            {
                try
                {
                    await patch.FinishApplyAsync(this.DataSource, localPath, updateSuccess, token);
                    this.RaisePatchProgessChanged(++currentPatchCount / totalPatchCount, patch.Version);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            return this.Equals(obj as Update);
        }

        public bool Equals(Update other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;

            return (this.Count == other.Count) && this.Patches.SequenceEqual(other.Patches);
        }

        public IEnumerator<Patch> GetEnumerator()
        {
            return this.Patches.GetEnumerator();
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
                hash = hash * 486187739 + this.LatestVersion.GetHashCode();
                hash = hash * 486187739 + this.Patches.GetHashCode();
                return hash;
            }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context != null)
            {
                this.DataSource = context.Context as IDataSource;
            }
        }

        public bool UpdatesAvailable(Version currentVersion)
        {
            return this.LatestVersion > currentVersion;
        }

        private void RaisePatchProgessChanged(int percentage, object state = null)
        {
            ProgressChangedEventHandler handler = this.PatchProgressChanged;
            if (handler != null)
            {
                handler(this, new ProgressChangedEventArgs(percentage, state));
            }
        }

        public static async Task<Update> LoadFromAsync(IDataSource dataSource, CancellationToken token)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);

            token.ThrowIfCancellationRequested();
            return await dataSource.GetUpdateAsync(token);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.Patches != null);
        }
    }
}
