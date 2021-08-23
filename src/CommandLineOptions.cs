using System.Diagnostics;
using CommandLine;

namespace preshutdownnotify
{
    public class CommandLineOptions
    {
        [Option(longName: "start", Required = false, HelpText = "Path of powershell startup scirpt.", Default = "start.ps1")]
        public string StartScriptPath { get; set; }

        [Option(longName: "stop", Required = false, HelpText = "Path of powershell shutdown scirpt.", Default = "stop.ps1")]
        public string StopScriptPath { get; set; }
    }
}