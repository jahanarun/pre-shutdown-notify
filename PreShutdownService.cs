using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace preshutdownnotify
{
    public class PreShutdownService : ServiceBase
    {
        private const int SERVICEACCEPTPRESHUTDOWN = 0x100;
        private const int SERVICECONTROLPRESHUTDOWN = 0xf;
        private readonly CommandLineOptions opts;

        public PreShutdownService(CommandLineOptions opts)
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                try
                {
                    FieldInfo serviceAcceptedCommands = typeof(ServiceBase).GetField("acceptedCommands", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    int v = (int)serviceAcceptedCommands.GetValue(this);
                    int xi = v;
                    serviceAcceptedCommands.SetValue(this, xi | SERVICEACCEPTPRESHUTDOWN);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("preshutdownnotify", ex.Message, EventLogEntryType.Error, 12100, short.MaxValue);
                }
            }

            this.opts = opts;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry("preshutdownnotify", "preshutdownnotify service starting", EventLogEntryType.Information, 12100, short.MaxValue);
                new PowershellExecuter(opts).RunStartScript().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("preshutdownnotify", e.Message, EventLogEntryType.Error, 12100, short.MaxValue);
            }
        }

        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry("preshutdownnotify", "preshutdownnotify service stopping", EventLogEntryType.Information, 12100, short.MaxValue);
                new PowershellExecuter(opts).RunStopScript().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("preshutdownnotify", e.Message, EventLogEntryType.Error, 12100, short.MaxValue);
                EventLog.WriteEntry("preshutdownnotify", e.StackTrace, EventLogEntryType.Error, 12100, short.MaxValue);
            }
        }

        protected override void OnCustomCommand(int command)
        {
            try
            {
                EventLog.WriteEntry("preshutdownnotify", $"Code :{command}", EventLogEntryType.Information, 12100, short.MaxValue);

                if (command == SERVICECONTROLPRESHUTDOWN)
                {
                    new PowershellExecuter(opts).RunStopScript().GetAwaiter().GetResult();
                    EventLog.WriteEntry("preshutdownnotify", $"Completed Custom Command {command}", EventLogEntryType.Information, 12100, short.MaxValue);
                }
                else
                {
                    base.OnCustomCommand(command);
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("preshutdownnotify", e.Message, EventLogEntryType.Error, 12100, short.MaxValue);
            }
        }
    }
}