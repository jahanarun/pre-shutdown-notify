using System.Diagnostics;
using CommandLine;

namespace preshutdownnotify
{
    public class CommandLineOptions
    {
        [Value(index: 0, Required = false, HelpText = "Path of powershell scirpt.", Default = "the-script-to-run-on-preshutdown.ps1")]
        public string Path { get; set; }
    }
}