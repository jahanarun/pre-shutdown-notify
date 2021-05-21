using System.Diagnostics;
using CommandLine;

namespace pre_shutdown_notify
{
    public class CommandLineOptions
    {
        private bool isConsole;

        [Value(index: 0, Required = false, HelpText = "Path of powershell scirpt.")]
        public string Path { get; set; }

        [Option(longName: "console", Required = false, HelpText = "Run as console app.", Default = false)]
        public bool IsConsole { get => isConsole || Debugger.IsAttached; set => isConsole = value; }
    }
}