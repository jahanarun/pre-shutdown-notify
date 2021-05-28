using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace preshutdownnotify
{
    internal class PowershellExecuter
    {
        private readonly CommandLineOptions opts;

        public PowershellExecuter(CommandLineOptions opts)
        {
            this.opts = opts;
        }

        public async Task ExecuteAsync()
        {
            string path;

            if (Path.IsPathFullyQualified(opts.Path))
            {
                EventLog.WriteEntry("preshutdownnotify", $"opt.Path: {opts.Path}", EventLogEntryType.Information, 12100, short.MaxValue);
                path = opts.Path;
            }
            else
            {
                EventLog.WriteEntry("preshutdownnotify", $"AppContext: {AppContext.BaseDirectory}", EventLogEntryType.Information, 12100, short.MaxValue);
                path = Path.Combine(AppContext.BaseDirectory, opts.Path);
            }

            EventLog.WriteEntry("preshutdownnotify", $"Final Path: {path}", EventLogEntryType.Information, 12100, short.MaxValue);
            using var ps = PowerShell.Create();

            var scriptContents = File.ReadAllText(path);

            // specify the script code to run.
            ps.AddScript(scriptContents);

            // specify the parameters to pass into the script.
            // ps.AddParameters(scriptParameters);

            // execute the script and await the result.
            var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

            EventLog.WriteEntry("preshutdownnotify", "Completed!!!", EventLogEntryType.Information, 12100, short.MaxValue);

            // print the resulting pipeline objects to the console.
            foreach (var item in pipelineObjects)
            {
                EventLog.WriteEntry("preshutdownnotify", item.BaseObject.ToString(), EventLogEntryType.Information, 12100, short.MaxValue);
            }
        }
    }
}