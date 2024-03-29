﻿using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Parser;
using KzA.HEXEH.Core.Parser.Common;
using KzA.HEXEH.Core.Parser.Common.String;
using KzA.HEXEH.Core.Parser.Networking;
using KzA.HEXEH.Core.Parser.Windows;
using System.Text;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test
{
    public class TestParser(ITestOutputHelper output) : TestBase(output)
    {
        [Fact]
        public void TestFqdn()
        {
            var data = new byte[] { 0x03, 0x61, 0x62, 0x63, 0x07, 0x63, 0x6F, 0x6E, 0x74, 0x6F, 0x73, 0x6F, 0x03, 0x63, 0x6F, 0x6D, 0x00 };
            var parser = new FqdnParser();
            var result = parser.Parse(data, null);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestFqdnList()
        {
            var data = PrepareData("06 67 6F 6F 67 6C 65 03 63 6F 6D 00 04 74 65 73 74 C0 00 09 6D 69 63 72 6F 73 6F 66 74 C0 07 04 69 70 76 36 C0 13");
            var parser = new FqdnListParser();
            var result = parser.Parse(data, null);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestIPv4Addr()
        {
            var data = new byte[] { 0xa0, 0xb0, 0xc0, 0xd0 };
            var parser = new IPv4AddrParser();
            var result = parser.Parse(data, null);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestIPv6Addr()
        {
            var data1 = new byte[] { 0xa0, 0xb0, 0xc0, 0xd0,
                                     0xa0, 0xb0, 0xc0, 0xd0,
                                     0x00, 0x00, 0x00, 0x00,
                                     0xa0, 0x00, 0x00, 0x00};
            var data2 = new byte[] { 0xa0, 0xb0, 0xc0, 0xd0,
                                     0xa0, 0xb0, 0xc0, 0xd0,
                                     0x01, 0x02, 0x03, 0x04,
                                     0x00, 0x00, 0x00, 0x00};
            var parser = new IPv6AddrParser();
            var result1 = parser.Parse(data1);
            var result2 = parser.Parse(data2);
            Output.WriteLine(result1.ToStringVerbose());
            Output.WriteLine(result2.ToStringVerbose());
        }

        [Fact]
        public void TestMacAddr()
        {
            var data = new byte[] { 0xa0, 0xb0, 0xc0, 0xd0, 0xe0, 0xf0 };
            var parser = new MacAddrParser();
            var result = parser.Parse(data, null);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestString()
        {
            var data = new byte[] { 0x41, 0x00, 0x20, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x7A, 0x00, 0x79, 0x00, 0x20, 0x00, 0x66, 0x00, 0x6F, 0x00, 0x78, 0x00, 0x20, 0x00, 0x6A, 0x00, 0x75, 0x00, 0x6D, 0x00, 0x70, 0x00, 0x73, 0x00, 0x20, 0x00, 0x6F, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x61, 0x00, 0x20, 0x00, 0x62, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x6E, 0x00, 0x20, 0x00, 0x66, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x63, 0x00, 0x65, 0x00 };
            var parser = new StringParser();
            parser.SetOptions(new Dictionary<string, object>()
            { {"Encoding", Encoding.Unicode } });
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestNullTermincatedString()
        {
            var data = new byte[] { 0x41, 0x20, 0x6C, 0x61, 0x7A, 0x79, 0x20, 0x66, 0x6F, 0x78, 0x20, 0x6A, 0x75, 0x00, 0x6D, 0x70, 0x73, 0x20, 0x6F, 0x76, 0x65, 0x72, 0x20, 0x61, 0x20, 0x62, 0x72, 0x6F, 0x77, 0x6E, 0x20, 0x66, 0x65, 0x6E, 0x63, 0x65, 0x00 };
            var parser = new NullTerminatedAsciiStringParser();
            var result = parser.Parse(data, out var read);
            Assert.Equal(14, read);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestLengthedObject()
        {
            var data = new byte[] { 0x46, 0x41, 0x00, 0x20, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x7A, 0x00, 0x79, 0x00, 0x20, 0x00, 0x66, 0x00, 0x6F, 0x00, 0x78, 0x00, 0x20, 0x00, 0x6A, 0x00, 0x75, 0x00, 0x6D, 0x00, 0x70, 0x00, 0x73, 0x00, 0x20, 0x00, 0x6F, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x61, 0x00, 0x20, 0x00, 0x62, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x6E, 0x00, 0x20, 0x00, 0x66, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x63, 0x00, 0x65, 0x00 };
            var parser = new LengthedObjectParser();
            var stringParserOpt = new Dictionary<string, object>()
            { {"Encoding", Encoding.Unicode } };
            parser.SetOptions(new Dictionary<string, object>()
            {
                {"LenOfLen", 1 },
                {"ObjectParser", "Common.String.String" },
                {"ParserOptions", stringParserOpt },
            });
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestLengthedString()
        {
            var data = new byte[] { 0x46, 0x41, 0x00, 0x20, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x7A, 0x00, 0x79, 0x00, 0x20, 0x00, 0x66, 0x00, 0x6F, 0x00, 0x78, 0x00, 0x20, 0x00, 0x6A, 0x00, 0x75, 0x00, 0x6D, 0x00, 0x70, 0x00, 0x73, 0x00, 0x20, 0x00, 0x6F, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x61, 0x00, 0x20, 0x00, 0x62, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x6E, 0x00, 0x20, 0x00, 0x66, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x63, 0x00, 0x65, 0x00 };
            var parser = new LengthedStringParser();
            parser.SetOptions(new Dictionary<string, object>()
            {
                {"LenOfLen", 1 },
                {"Encoding", Encoding.Unicode }
            });
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestUnixTimestamp()
        {
            var data = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd };
            var parser = new UnixTimeParser();
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestFileTime()
        {
            var data = PrepareData("01da4888b9e5acc0");
            var parser = new FILETIMEParser();
            parser.SetOptions(new() { { "BigEndian", true } });
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }

        [Fact]
        public void TestSystemTime()
        {
            var data = PrepareData("e7 07 0b 00 03 00 08 00 0b 00 3b 00 08 00 0f 00");
            var parser = new SYSTEMTIMEParser();
            var result = parser.Parse(data);
            Output.WriteLine(result.ToStringVerbose());
        }
    }
}
