using KzA.HEXEH.Core.Output;
using Serilog;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.HEXEH.Core.Parser.Windows
{
    internal class FILETIMEParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, 8, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 8;
            return Parse(Input, 0, 8, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, 8, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 8;
            return Parse(Input, Offset, 8, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[FILETIMEParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (Length != 8) throw new ArgumentException("FILETIME length must be 8");

                var filetime = BinaryPrimitives.ReadInt64LittleEndian(Input.Slice(Offset, 8));
                var datetime = DateTime.FromFileTime(filetime);
                var res = new DataNode()
                {
                    Label = "FILETIME",
                    Value = datetime.ToString()
                };
                Log.Debug("[FILETIMEParser] Parsed 8 bytes");
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
            throw new NotSupportedException();
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotSupportedException();
        }
    }
}
