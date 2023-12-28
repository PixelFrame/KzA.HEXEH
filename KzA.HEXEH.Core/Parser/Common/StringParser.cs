using KzA.HEXEH.Core.Output;
using Serilog;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class StringParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;
        private Encoding encoding = Encoding.UTF8;

        public override Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                {"Encoding", typeof(Encoding)},
            };
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, Input.Length, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            throw new NotSupportedException("Unable to infer data length");
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, Input.Length - Offset, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            throw new NotSupportedException("Unable to infer data length");
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[StringParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                var res = new DataNode()
                {
                    Label = $"String ({encoding.EncodingName})",
                    Value = encoding.GetString(Input.Slice(Offset, Length).ToArray()),
                    Index = Offset,
                    Length = Length
                };
                Log.Debug("[StringParser] Parsed {Length} bytes", Length);
                ParseStack!.PopEx();
                return res;
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                if (encodingObj is Encoding _encoding) { encoding = _encoding; }
                else
                {
                    throw new ArgumentException("Invalid Option: Encoding");
                }
            }
            else
            {
                throw new ArgumentException("Encoding not provided");
            }
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                encoding = Encoding.GetEncoding(encodingObj);
            }
            else
            {
                throw new ArgumentException("Encoding not provided");
            }
        }
    }
}
