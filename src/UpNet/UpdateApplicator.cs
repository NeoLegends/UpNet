using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpNet
{
    public class UpdateApplicator
    {
        public static void Apply(ApplyOptions applyOptions)
        {
            Contract.Requires<ArgumentNullException>(applyOptions != null);

            throw new NotImplementedException();
        }
    }
}
