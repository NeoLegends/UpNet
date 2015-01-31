using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace UpNet
{
    class Program
    {
        static int Main(string[] args)
        {
            string invokedVerbName = null;
            object invokedVerb = null;
            Options options = new Options();

            if (!Parser.Default.ParseArguments(args, options, (verbName, verbObject) => { invokedVerbName = verbName; invokedVerb = verbObject; }))
            {
                Console.WriteLine("There was an error while parsing the command line options. Try again.");
                return -1;
            }

            ApplyOptions applyVerb = invokedVerb as ApplyOptions;
            if (applyVerb != null)
            {
                UpdateApplicator.Apply(applyVerb);
                return 0;
            }

            CreateOptions createVerb = invokedVerb as CreateOptions;
            if (createVerb != null)
            {
                UpdateCreator.Create(createVerb);
                return 0;
            }

            Console.WriteLine("The parsed verb was not found. This is undefined behaviour.");
            return -2;
        }
    }
}
