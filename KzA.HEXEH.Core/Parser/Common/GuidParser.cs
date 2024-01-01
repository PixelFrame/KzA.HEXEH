using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;

namespace KzA.HEXEH.Core.Parser.Common
{
    internal class GuidParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 16;
            return Parse(Input, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            Log.Debug("[GuidParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                var guid = new Guid(Input.Slice(Offset, 16));
                Log.Debug("[GuidParser] Parsed 16 bytes");
                ParseStack!.PopEx();
                return new DataNode()
                {
                    Label = "GUID",
                    Value = guid.ToString(),
                    Index = Offset,
                    Length = 16,
                };
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Unable to create GUID from given data", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 16;
            return Parse(Input, Offset, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            if (Length != 16) throw new ArgumentException("GUID length must be 16");
            return Parse(Input, Offset, ParseStack);
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
