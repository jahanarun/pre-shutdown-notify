using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Threading.Tasks;
using CommandLine;
using System.ServiceProcess;

namespace preshutdownnotify
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {

            return await Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(async (opts) =>
                {

                    ServiceBase[] servicesToRun;
                    servicesToRun = new ServiceBase[]
                    {
                        new PreShutdownService(opts)
                    };
                    ServiceBase.Run(servicesToRun);
                    return 0;
                },
                errs => Task.FromResult(-1)); // Invalid


            
        }
    }
}
