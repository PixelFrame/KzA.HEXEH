using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class FqdnParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, Input.Length, ParseStack);
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
            Log.Debug("[FqdnParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                var start = Offset;
                var nullReached = false;
                var readLen = true;
                var len = 0;
                var sb = new StringBuilder();
                var loopCnt = 0;
                while (!nullReached)
                {
                    if (readLen)
                    {
                        readLen = false;
                        len = Input[Offset];
                        sb.Append($"({len})");
                        Offset++;
                        if (len == 0) { nullReached = true; }
                    }
                    else
                    {
                        readLen = true;
                        sb.Append(Encoding.ASCII.GetString(Input.Slice(Offset, len).ToArray()));
                        Offset += len;
                    }
                    if (++loopCnt > Global.LoopMax)
                    {
                        throw new StackOverflowException("Array loop exceeds limitation, please verify if data is valid or adjust the limitation");
                    }
                }
                Read = Offset - start;
                var result = new DataNode()
                {
                    Label = "FQDN",
                    Value = sb.ToString(),
                    Index = start,
                    Length = Read
                };
                Log.Debug("[FqdnParser] Parsed {Read} bytes", Read);
                ParseStack!.PopEx();
                return result;
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
                res.Children.Add(paddingNode);
            }
            if (read > Length)
            {
                Log.Error("[FqdnParser] Actual FQDN length exceeding given length");
                ParseStack!.Push(GetType().FullName ?? GetType().Name);
                throw new ParseLengthMismatchException("Actual FQDN length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
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
