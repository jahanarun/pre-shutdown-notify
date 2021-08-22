using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace preshutdownnotify
{
    internal class PowershellExecuter
    {
        private readonly CommandLineOptions opts;

        public PowershellExecuter(CommandLineOptions opts)
        {
            this.opts = opts;
        }

        public Task RunStartScript()
        {
            return ExecuteAsync(opts.StartScriptPath);
        }

        public Task RunStopScript()
        {
            return ExecuteAsync(opts.StopScriptPath);
        }

        private static async Task ExecuteAsync(string path)
        {
            if (Path.IsPathFullyQualified(path))
            {
                EventLog.WriteEntry("preshutdownnotify", $"opt.Path: {path}", EventLogEntryType.Information, 12100, short.MaxValue);
            }
            else
            {
                EventLog.WriteEntry("preshutdownnotify", $"AppContext: {AppContext.BaseDirectory}", EventLogEntryType.Information, 12100, short.MaxValue);
                path = Path.Combine(AppContext.BaseDirectory, path);
            }

            EventLog.WriteEntry("preshutdownnotify", $"Final Path: {path}", EventLogEntryType.Information, 12100, short.MaxValue);

            var initialState = InitialSessionState.CreateDefault();
            initialState.ImportPSModule(new string[] { "Hyper-V" });
            initialState.ThrowOnRunspaceOpenError = true;
            Runspace runspace = RunspaceFactory.CreateRunspace(initialState);
            runspace.Open();

            var moduleError = runspace.SessionStateProxy.PSVariable.GetValue("Error");
            if (!string.IsNullOrEmpty(moduleError.ToString()))
            {
                EventLog.WriteEntry("preshutdownnotify", $"Module error: {moduleError}", EventLogEntryType.Information, 12100, short.MaxValue);
            }


            using var ps = PowerShell.Create();
            ps.Runspace = runspace;

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