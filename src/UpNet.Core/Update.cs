using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpNet.Core.DataSource;

namespace UpNet.Core
{
    [DataContract]
    public class Update : IEnumerable<Patch>
    {
        public event ProgressChangedEventHandler UpdateProgessChanged;

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
                return (patches != null) ? patches.OrderByDescending(patch => patch.Version).First().Version : null;
            }
        }

        private Update() { }

        public Update(IEnumerable<Patch> patches)
        {
            Contract.Requires<ArgumentNullException>(patches != null);

            this.Patches = patches.ToImmutableList();
        }

        public async Task Apply(IDataSource dataSource, Version currentVersion)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckForUpdates(IDataSource dataSource, Version currentVersion)
        {
            throw new NotImplementedException();
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

        private void RaiseProgessChanged(int percentage, object state = null)
        {
            ProgressChangedEventHandler handler = this.UpdateProgessChanged;
            if (handler != null)
            {
                handler(this, new ProgressChangedEventArgs(percentage, state));
            }
        }

        public static async Task<Update> LoadFrom(IDataSource dataSource)
        {
            Contract.Requires<ArgumentNullException>(dataSource != null);

            return new Update(await dataSource.GetUpdateAsync());
        }

        public static implicit operator Update(Patch[] patches)
        {
            return (patches != null) ? new Update(patches) : null;
        }
    }
}
