using KzA.HEXEH.Core.Utility;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test
{
    public class TestUtility(ITestOutputHelper output) : TestBase(output)
    {
        [Fact]
        public void TestPsRunScriptForStringResult()
        {
            var psout = PsScriptRunner.RunScriptForStringResult(
                @" $hours = [BitConverter]::ToUInt32($value, 0); return [datetime]::Parse('1601/1/1 0:0:0').AddHours($hours)",
                [0x1D, 0x93, 0x38, 0x00]);
            Output.WriteLine(psout);
        }
    }
}
