using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Threading.Tasks;
using CommandLine;

namespace preshutdownnotify
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(async (opts) =>
                {
                    var builder = CreateHostBuilder(args, opts);
                    if (opts.IsConsole)
                    {
                        await builder.RunConsoleAsync();
                    }
                    else
                    {
                        await builder.RunAsServiceAsync();
                    }
                    return 0;
                },
                errs => Task.FromResult(-1)); // Invalid arguments
        }

        public static IHostBuilder CreateHostBuilder(string[] args, CommandLineOptions opts) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureLogging(configureLogging => configureLogging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information))
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddSingleton(opts);
                   services.AddHostedService<PowershellExecuter>()
                       .Configure<EventLogSettings>(config =>
                       {
                           config.LogName = "Pre-shutdown Service";
                           config.SourceName = "Pre-shutdown Service Source";
                       });
               }).UseWindowsService();
    }
}
