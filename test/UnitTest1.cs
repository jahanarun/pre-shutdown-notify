using preshutdownnotify;
using System.Threading.Tasks;
using Xunit;

namespace pre_shutdown_notify
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1Async()
        {
            var opt = new CommandLineOptions();
            opt.StartScriptPath = "start.ps1";
            var ps = new PowershellExecuter(opt);
            await ps.RunStartScript();

        }
    }
}
