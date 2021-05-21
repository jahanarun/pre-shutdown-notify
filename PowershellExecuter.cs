using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace pre_shutdown_notify
{
    internal class PowershellExecuter : IHostedService
    {
        private readonly CommandLineOptions opts;
        private readonly ILogger<PowershellExecuter> logger;

        public PowershellExecuter(CommandLineOptions opts, ILogger<PowershellExecuter> logger)
        {
            this.opts = opts;
            this.logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string path;
            if (Path.IsPathFullyQualified(opts.Path))
            {
                path = opts.Path;
            }
            else
            {
                path = Path.Combine(AppContext.BaseDirectory, opts.Path);
            }

            using var ps = PowerShell.Create();

            var scriptContents = File.ReadAllText(path);

            // specify the script code to run.
            ps.AddScript(scriptContents);

            // specify the parameters to pass into the script.
            // ps.AddParameters(scriptParameters);

            // execute the script and await the result.
            var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

            // print the resulting pipeline objects to the console.
            foreach (var item in pipelineObjects)
            {
                logger.LogInformation(item.BaseObject.ToString());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}