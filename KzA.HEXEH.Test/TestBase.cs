using Serilog;
using Serilog.Enrichers.WithCaller;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test
{
    public class TestBase
    {
        protected readonly ITestOutputHelper Output;

        const string LOGGING_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Caller}] {Message:lj}{NewLine}{Exception}";

        public TestBase(ITestOutputHelper output)
        {
            Output = output;
            var loggingPath = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "./Log", "HEXEH", "Test-.Log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithCaller(false, 1)
                .WriteTo.TestOutput(Output, outputTemplate: LOGGING_TEMPLATE)
                .WriteTo.File(loggingPath, rollingInterval: RollingInterval.Day, outputTemplate: LOGGING_TEMPLATE)
                .CreateLogger();
        }
    }
}
