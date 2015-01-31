using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace UpNet
{
    public class Options
    {
        [VerbOption("apply", HelpText = "Apply an update to a directory.")]
        public ApplyOptions ApplyOptions { get; set; }

        [VerbOption("create", HelpText = "Create an update file from a directory.")]
        public CreateOptions DiffOptions { get; set; }
    }

    public abstract class VerbOptions
    {
        [ParserState]
        public IParserState ParserState { get; set; }

        [Option('v', "verbose", DefaultValue = false, HelpText = "Output verbose progress information.")]
        public bool Verbose { get; set; }
    }

    public class ApplyOptions : VerbOptions
    {
        [Option('p', "post", HelpText = "The application / shell script to execute after the update has been completed.")]
        public string PostUpdate { get; set; }

        [ValueOption(0)]
        [Option('d', "dir", HelpText = "The directory to apply the update to.")]
        public string BaseDirectory { get; set; }

        [Option('t', "time", HelpText = "The time to wait before the update will be applied (in milliseconds).")]
        public int InitialWait { get; set; }

        [Option('f', "file", HelpText = "The update file.")]
        public string UpdateFilePath { get; set; }
    }

    public class CreateOptions : VerbOptions
    {
        [Option('b', "base", HelpText = "The update file to take as base.")]
        public string BaseUpdateFile { get; set; }

        [ValueOption(0)]
        [Option('d', "dir", HelpText = "The base directory.")]
        public string Directory { get; set; }

        [Option('o', "output", Required = true, HelpText = "The file to write the new update file to.")]
        public string OutputFile { get; set; }
    }
}
