using System.Diagnostics;
using CommandLine;

namespace preshutdownnotify
{
    public class CommandLineOptions
    {
        [Value(index: 0, Required = false, HelpText = "Path of powershell startup scirpt.", Default = "start.ps1")]
        public string StartScriptPath { get; set; }

        [Value(index: 1, Required = false, HelpText = "Path of powershell shutdown scirpt.", Default = "stop.ps1")]
        public string StopScriptPath { get; set; }
    }
}