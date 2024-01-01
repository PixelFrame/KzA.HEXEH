using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Common.String
{
    public class NullTerminatedAsciiStringParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Input.Length, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, Input.Length - Offset, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[NullTerminatedAsciiStringParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                byte[] terminator = [0x00];
                var len = Input.IndexOf(terminator);
                Read = len + 1;
                var res = new DataNode()
                {
                    Label = $"String ({Encoding.ASCII.EncodingName})",
                    Value = Encoding.ASCII.GetString(Input.Slice(Offset, len).ToArray()),
                    Index = Offset,
                    Length = Read
                };
                Log.Debug("[NullTerminatedAsciiStringParser] Parsed {Read} bytes", Read);
                ParseStack!.PopEx();
                return res;
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            var res = Parse(in Input, Offset, out int read, ParseStack);
            if (read < Length)
            {
                var paddingNode = new DataNode()
                {
                    Label = "Padding (Unread Bytes)",
                    Value = BitConverter.ToString(Input.Slice(Offset + read, Length - read).ToArray()),
                    Index = Offset + read,
                    Length = Length - read,
                };
                res.Length = Length;
                res.Children.Add(paddingNode);
            }
            if (read > Length)
            {
                Log.Error("[NullTerminatedAsciiStringParser] Actual string length exceeding given length");
                ParseStack!.Push(GetType().FullName ?? GetType().Name);
                throw new ParseLengthMismatchException("Actual string length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
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
