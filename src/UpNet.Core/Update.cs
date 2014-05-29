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
    public class Update : IEnumerable<Patch>
    {
        public event ProgressChangedEventHandler FileProgressChanged;

        public event ProgressChangedEventHandler PatchProgressChanged;

        [IgnoreDataMember]
        public IDataSource DataSource { get; private set; }

        [DataMember]
        public IEnumerable<Patch> Patches { get; private set; }

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

        private Update() { }

        public Update(IEnumerable<Patch> patches)
            : this(patches, null)
        {
            Contract.Requires<ArgumentNullException>(patches != null);
        }

        public Update(IEnumerable<Patch> patches, IDataSource dataSource)
        {
            Contract.Requires<ArgumentNullException>(patches != null);

            this.DataSource = dataSource;
            this.Patches = patches.ToImmutableList();
        }

        public async Task Apply(String localPath, Version currentVersion)
        {
            Contract.Requires<ArgumentNullException>(localPath != null);
            Contract.Requires<ArgumentNullException>(currentVersion != null);
            Contract.Requires<InvalidOperationException>(this.DataSource != null);

            IEnumerable<Patch> patchesToApply = this.Patches.Where(patch => patch.Version > currentVersion).OrderBy(patch => patch.Version);
            int totalPatchCount = Math.Max(patchesToApply.Count(), 1);
            int totalFileCount = Math.Max(patchesToApply.Sum(patch => patch.ChangeCount), 1);
            int currentPatchCount = 0;
            int currentFileCount = 0;

            foreach (Patch patch in patchesToApply)
            {
                patch.UpdateProgressChanged += (s, e) => this.RaiseFileProgessChanged(Interlocked.Increment(ref currentFileCount) / totalFileCount, e.UserState);
                await patch.Apply(this.DataSource, localPath);
                this.RaisePatchProgessChanged(Interlocked.Increment(ref currentPatchCount) / totalPatchCount, patch.Version);
            }
        }

        public bool UpdatesAvailable(Version currentVersion)
        {
            return this.LatestVersion > currentVersion;
        }

        public IEnumerator<Patch> GetEnumerator()
        {
            IEnumerable<Patch> patches = this.Patches;
            return (patches != null) ? patches.GetEnumerator() : null;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void RaiseFileProgessChanged(int percentage, object state = null)
        {
            ProgressChangedEventHandler handler = this.FileProgressChanged;
            if (handler != null)
            {
                handler(this, new ProgressChangedEventArgs(percentage, state));
            }
        }

        private void RaisePatchProgessChanged(int percentage, object state = null)
        {
            ProgressChangedEventHandler handler = this.PatchProgressChanged;
            if (handler != null)
            {
                handler(this, new ProgressChangedEventArgs(percentage, state));
            }
        }

        public static async Task<Update> LoadFrom(IDataSource dataSource)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);

            return new Update(await dataSource.GetUpdateAsync(), dataSource);
        }
    }
}
