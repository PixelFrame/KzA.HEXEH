using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class UnixTimeParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[UnixTimeParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                var etp = new ElapsedTimeParser();
                etp.SetOptions(new()
                {
                    { "StartTime", DateTime.Parse("1970/01/01 0:0:0") },
                    { "Unit", "s" },
                    { "Length", 4 },
                });
                var res = etp.Parse(Input, Offset, ParseStack);
                res.Label = "Unix Timestamp";
                Read = 4;
                Log.Debug("[UnixTimeParser] Parsed {Read} bytes", Read);
                ParseStack.PopEx();
                return res;
            }
            catch (ParseFailureException e)
            {
                throw new ParseFailureException("Failed to parse inner object", e.ParserStackPrint, Offset, e);
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Unable to parse the data to time", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            if (Length != 4) throw new ArgumentException("Unix Timestamp length must be 4");
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
