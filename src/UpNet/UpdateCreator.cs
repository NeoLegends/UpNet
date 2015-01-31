using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpNet
{
    public class UpdateCreator
    {
        public static void Create(CreateOptions createOptions)
        {
            Contract.Requires<ArgumentNullException>(createOptions != null);

            throw new NotImplementedException();
        }
    }
}
