using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class MacAddrParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, 6, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 6;
            return Parse(Input, 0, 6, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, 6, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 6;
            return Parse(Input, Offset, 6, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[MacAddrParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (Length != 6) { throw new ArgumentException("Length of MAC Address should be 6."); }
                if (Input.Length - Offset < 6) { throw new ArgumentException("Data too short."); }
                var head = new DataNode()
                {
                    Label = "MAC Address",
                    Value = $"{Input[Offset]:X2}:{Input[Offset + 1]:X2}:{Input[Offset + 2]:X2}:{Input[Offset + 3]:X2}:{Input[Offset + 4]:X2}:{Input[Offset + 5]:X2}",
                    Index = Offset,
                    Length = 6,
                };
                Log.Debug("[IPv4AddrParser] Parsed 6 bytes");
                ParseStack!.PopEx();
                return head;
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }
    }
}
