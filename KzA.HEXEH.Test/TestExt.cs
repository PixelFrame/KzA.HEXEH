using KzA.HEXEH.Core.Parser;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test
{
    public class TestExt(ITestOutputHelper output) : TestBase(output)
    {
        [Fact]
        public void TestPwshParser()
        {
            var script = @"
function Parse {
    param (
        [byte[]] $value,
        [int] $index
    )
    $varr = @(0,0,0,0) 
    [array]::Copy($value, $varr, 4)
    $res = [DataNode]::new('PwshTestProp', [System.BitConverter]::ToString($varr), $index, 4)
    return (4, $res);
}";
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            var pwshparser = ParserManager.InstantiateParserByFullName("KzA.HEXEH.Ext.Pwsh.Pwsh", new() { { "Script", script } });
            var dn = pwshparser.Parse(data, 3);
            Output.WriteLine(dn.ToString());
        }
    }
}
