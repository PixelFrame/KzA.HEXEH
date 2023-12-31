﻿using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System.Buffers.Binary;

namespace KzA.HEXEH.Core.Parser.Windows
{
    internal class FILETIMEParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;

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

                var filetime = BigEndian ? BinaryPrimitives.ReadInt64BigEndian(Input.Slice(Offset, 8)) : BinaryPrimitives.ReadInt64LittleEndian(Input.Slice(Offset, 8));
                var datetime = DateTime.FromFileTime(filetime);
                var res = new DataNode()
                {
                    Label = "FILETIME",
                    Value = datetime.ToString(),
                    Index = Offset,
                    Length = 8,
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
            base.SetOptions(Options);
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            base.SetOptionsFromSchema(Options);
        }
    }
}
