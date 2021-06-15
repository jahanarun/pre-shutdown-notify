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
            // ps.Streams.Error.ReadAll

            // specify the parameters to pass into the script.
            // ps.AddParameters(scriptParameters);

            // execute the script and await the result.
            ps.Streams.Error.DataAdded += (sender, args) =>
            {
                ErrorRecord err = ((PSDataCollection<ErrorRecord>)sender)[args.Index];
                EventLog.WriteEntry("preshutdownnotify", $"ERROR: {err}", EventLogEntryType.Information, 12100, short.MaxValue);
            };

            ps.Streams.Warning.DataAdded += (sender, args) =>
            {
                WarningRecord warning = ((PSDataCollection<WarningRecord>)sender)[args.Index];
                EventLog.WriteEntry("preshutdownnotify", $"WARNING: {warning}", EventLogEntryType.Information, 12100, short.MaxValue);
            };
            var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
            pipelineObjects.DataAdded += (sender, args) =>
            {
                PSObject output = ((PSDataCollection<PSObject>)sender)[args.Index];
                EventLog.WriteEntry("preshutdownnotify", $"OUTPUT: {output}", EventLogEntryType.Information, 12100, short.MaxValue);
            };


            // print the resulting pipeline objects to the console.
            foreach (var item in pipelineObjects)
            {
                EventLog.WriteEntry("preshutdownnotify", item.BaseObject.ToString(), EventLogEntryType.Information, 12100, short.MaxValue);
            }
            if (ps.Streams.Error.Count > 0)
            {
                EventLog.WriteEntry("preshutdownnotify", $"Error Count: {ps.Streams.Error.Count}", EventLogEntryType.Information, 12100, short.MaxValue);
                foreach (var error in ps.Streams.Error.ReadAll())
                {
                    EventLog.WriteEntry("preshutdownnotify", $"Error Message: {error.ErrorDetails.Message}", EventLogEntryType.Information, 12100, short.MaxValue);
                }
                // error records were written to the error stream.
                // Do something with the error
            }
            EventLog.WriteEntry("preshutdownnotify", "Completed!!!", EventLogEntryType.Information, 12100, short.MaxValue);
        }
    }
}