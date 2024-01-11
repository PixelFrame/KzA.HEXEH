using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System.Buffers.Binary;

namespace KzA.HEXEH.Core.Parser.Windows
{
    public class SYSTEMTIMEParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

        private static readonly string[] _dow = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, 16, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 16;
            return Parse(Input, 0, 16, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, 16, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Read = 16;
            return Parse(Input, Offset, 16, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[SYSTEMTIMEParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (Length != 16) throw new ArgumentException("SYSTEMTIME length must be 16");

                var year = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                Offset += 2;
                var month = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                Offset += 2;
                var dayOfWeek = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                
                Offset += 2;
                var day = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                Offset += 2;
                var hour = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                Offset += 2;
                var minute = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                Offset += 2;
                var second = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                Offset += 2;
                var millisecond = BigEndian ? BinaryPrimitives.ReadInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadInt16LittleEndian(Input.Slice(Offset, 2));
                
                var res = new DataNode()
                {
                    Label = "SYSTEMTIME",
                    Value = $"{year}/{month}/{day} {_dow[dayOfWeek]} {hour}:{minute}:{second}.{millisecond}",
                    Index = Offset,
                    Length = 8,
                };
                Log.Debug("[SYSTEMTIMEParser] Parsed 16 bytes");
                ParseStack!.PopEx();
                return res;
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }
    }
}
