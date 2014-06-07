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
        public event ProgressChangedEventHandler PatchProgressChanged;

        private IDataSource _DataSource;

        [IgnoreDataMember]
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

            if (dataSource != null)
            {
                this.DataSource = dataSource;
            }
            this.Patches = patches.ToImmutableList();
        }

        public async Task ApplyAsync(String localPath, Version currentVersion)
        {
            Contract.Requires<ArgumentNullException>(localPath != null);
            Contract.Requires<ArgumentNullException>(currentVersion != null);
            Contract.Requires<InvalidOperationException>(this.DataSource != null);

            IEnumerable<Patch> patchesToApply = this.Patches.Where(patch => patch.Version > currentVersion).OrderBy(patch => patch.Version);
            bool updateSuccess = true;

            int totalPatchCount = Math.Max(patchesToApply.Count(), 1) * 2; // Times two because of two-stage updating
            int currentPatchCount = 0;

            try
            {
                foreach (Patch patch in patchesToApply)
                {
                    await patch.ApplyAsync(this.DataSource, localPath);
                    this.RaisePatchProgessChanged(++currentPatchCount / totalPatchCount, patch.Version);
                }
            }
            catch 
            {
                updateSuccess = false;
            }

            foreach (Patch patch in patchesToApply)
            {
                await patch.FinishApplyAsync(this.DataSource, localPath, updateSuccess);
                this.RaisePatchProgessChanged(++currentPatchCount / totalPatchCount, patch.Version);
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

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context != null)
            {
                this.DataSource = (IDataSource)context.Context;
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

        public static async Task<Update> LoadFromAsync(IDataSource dataSource)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);

            return await dataSource.GetUpdateAsync();
        }
    }
}
