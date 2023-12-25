using KzA.HEXEH.Core.Parser;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test
{
    public class TestDynamic(ITestOutputHelper output) : TestBase(output)
    {
        [Fact]
        public void TestEnum()
        {
            Core.Global.Initialize();
            foreach (var parser in ParserFinder.AvailableParsers)
            {
                Output.WriteLine($"{parser.Name} | {parser.FullName}");
            }
        }

        [Fact]
        public void TestDnsCountName()
        {
            var data = new byte[] { 0x11, 0x03, 0x03, 0x61, 0x62, 0x63, 0x07, 0x63, 0x6F, 0x6E, 0x74, 0x6F, 0x73, 0x6F, 0x03, 0x63, 0x6F, 0x6D, 0x00 };
            var parser = ParserFinder.InstantiateParserByName("DnsCountName");
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestDnsRecordDsAttribute_A()
        {
            Core.Global.Initialize();
            var data = new byte[] { 0x04, 0x00, 0x01, 0x00, 0x05, 0xF0, 0x00, 0x00, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x1D, 0x93, 0x38, 0x00, 0x0A, 0x02, 0x0A, 0x0D };
            var parser = ParserFinder.InstantiateParserByName("DnsRecordDsAttribute");
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }
    }
}
