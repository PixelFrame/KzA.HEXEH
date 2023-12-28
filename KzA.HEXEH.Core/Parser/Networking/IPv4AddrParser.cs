using KzA.HEXEH.Core.Output;
using Serilog;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class IPv4AddrParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, 4, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 4;
            return Parse(Input, 0, 4, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, 4, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 4;
            return Parse(Input, Offset, 4, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[IPv4AddrParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (Length != 4) { throw new ArgumentException("Length of IPv4 Address should be 4."); }
                if (Input.Length - Offset < 4) { throw new ArgumentException("Data too short."); }
                var head = new DataNode()
                {
                    Label = "IPv4 Address",
                    Value = $"{Input[Offset]}.{Input[Offset + 1]}.{Input[Offset + 2]}.{Input[Offset + 3]}",
                    Index = Offset,
                    Length = 4,
                };
                Log.Debug("[IPv4AddrParser] Parsed 4 bytes");
                ParseStack!.PopEx();
                return head;
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotSupportedException();
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotSupportedException();
        }
    }
}
