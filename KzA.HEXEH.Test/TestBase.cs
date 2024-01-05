using Serilog;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test
{
    public class TestBase
    {
        protected readonly ITestOutputHelper Output;

        const string LOGGING_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        private static bool LoggerCreated = false;

        public TestBase(ITestOutputHelper output)
        {
            Output = output;
            if (!LoggerCreated)
            {
                var loggingPath = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "./Log", "HEXEH", "Test-.Log");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    //.WriteTo.TestOutput(Output, outputTemplate: LOGGING_TEMPLATE)
                    .WriteTo.File(loggingPath, rollingInterval: RollingInterval.Day, outputTemplate: LOGGING_TEMPLATE)
                    .CreateLogger();
                LoggerCreated = true;
            }
        }
    }
}
