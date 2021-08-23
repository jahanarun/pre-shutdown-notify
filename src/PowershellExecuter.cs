using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using System.Threading.Tasks;

namespace preshutdownnotify
{
    public class PowershellExecuter
    {
        private readonly CommandLineOptions opts;

        public PowershellExecuter(CommandLineOptions opts)
        {
            this.opts = opts;
            EventLog.WriteEntry("preshutdownnotify", $"opt: {JsonSerializer.Serialize(opts)}", EventLogEntryType.Information, 12100, short.MaxValue);
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
                EventLog.WriteEntry("preshutdownnotify", $"opt.Path: {path}", EventLogEntryType.Information, 12101, short.MaxValue);
            }
            else
            {
                EventLog.WriteEntry("preshutdownnotify", $"AppContext: {AppContext.BaseDirectory}", EventLogEntryType.Information, 12102, short.MaxValue);
                path = Path.Combine(AppContext.BaseDirectory, path);
            }

            EventLog.WriteEntry("preshutdownnotify", $"Final Path: {path}", EventLogEntryType.Information, 12103, short.MaxValue);

            var initialState = InitialSessionState.CreateDefault();
            initialState.ImportPSModule(new string[] { "Hyper-V" });
            initialState.ThrowOnRunspaceOpenError = true;
            using Runspace runspace = RunspaceFactory.CreateRunspace(initialState);
            runspace.Open();

            var moduleErrors = runspace.SessionStateProxy.PSVariable.GetValue("Error") as ArrayList;
            if (moduleErrors.Count > 0)
            {
                foreach (var moduleError in moduleErrors)
                {
                    EventLog.WriteEntry("preshutdownnotify", $"Module error: {moduleError}", EventLogEntryType.Error, 12104, short.MaxValue);
                }
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
                EventLog.WriteEntry("preshutdownnotify", $"ERROR: {err}", EventLogEntryType.Error, 12105, short.MaxValue);
            };

            ps.Streams.Warning.DataAdded += (sender, args) =>
            {
                WarningRecord warning = ((PSDataCollection<WarningRecord>)sender)[args.Index];
                EventLog.WriteEntry("preshutdownnotify", $"WARNING: {warning}", EventLogEntryType.Information, 12106, short.MaxValue);
            };
            var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
            pipelineObjects.DataAdded += (sender, args) =>
            {
                PSObject output = ((PSDataCollection<PSObject>)sender)[args.Index];
                EventLog.WriteEntry("preshutdownnotify", $"OUTPUT: {output}", EventLogEntryType.Information, 12107, short.MaxValue);
            };


            // print the resulting pipeline objects to the console.
            foreach (var item in pipelineObjects)
            {
                EventLog.WriteEntry("preshutdownnotify", item.BaseObject.ToString(), EventLogEntryType.Information, 12108, short.MaxValue);
            }
            if (ps.Streams.Error.Count > 0)
            {
                EventLog.WriteEntry("preshutdownnotify", $"Error Count: {ps.Streams.Error.Count}", EventLogEntryType.Information, 12109, short.MaxValue);
                foreach (var error in ps.Streams.Error.ReadAll())
                {
                    EventLog.WriteEntry("preshutdownnotify", $"Error Message: {error.Exception.Message ?? error.ErrorDetails.Message}", EventLogEntryType.Information, 12110, short.MaxValue);
                }
                // error records were written to the error stream.
                // Do something with the error
            }
            EventLog.WriteEntry("preshutdownnotify", "Completed!!!", EventLogEntryType.Information, 12111, short.MaxValue);
        }
    }
}