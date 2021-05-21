using System.Diagnostics;
using CommandLine;

namespace preshutdownnotify
{
    public class CommandLineOptions
    {
        private bool isConsole;

        [Value(index: 0, Required = false, HelpText = "Path of powershell scirpt.", Default = "the-script-to-run-on-preshutdown.ps1")]
        public string Path { get; set; }

        [Option(longName: "console", Required = false, HelpText = "Run as console app.", Default = false)]
        public bool IsConsole { get => isConsole || Debugger.IsAttached; set => isConsole = value; }
    }
}