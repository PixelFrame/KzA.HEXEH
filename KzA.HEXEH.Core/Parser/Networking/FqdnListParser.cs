using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System;
using System.Buffers.Binary;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class FqdnListParser : ParserBase
    {
        const ushort INDEX_MAX = 0x3FFF;
        const ushort POINTER_MASK = 0xC000;
        const byte LABEL_LEN_MAX = 0x3F;

        public override ParserType Type => ParserType.Internal;

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, Input.Length, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            Read = Input.Length;
            return Parse(Input, 0, Input.Length, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            var len = Input.Length - Offset;
            return Parse(Input, Offset, len, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Read = Input.Length - Offset;
            return Parse(Input, Offset, Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[FqdnListParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                List<DataNode> fqdnNodes = [];
                var start = 0;
                while (Offset < start + Length)
                {
                    int elapsed = 0;
                    var passedPtr = new List<ushort>();
                    var name = ReadName(Input, Offset, ref elapsed, ref passedPtr);
                    var fqdnNode = new DataNode()
                    {
                        Label = "FQDN",
                        Value = name.Remove(name.Length - 1),
                        Index = Offset,
                        Length = elapsed
                    };
                    Offset += elapsed;
                    fqdnNodes.Add(fqdnNode);
                }
                var node = new DataNode()
                {
                    Label = "FQDN List",
                    Value = "",
                    Index = start,
                    Length = Length,
                    Children = fqdnNodes
                };
                Log.Debug("[FqdnListParser] Parsed {Length} bytes", Length);
                ParseStack!.PopEx();
                return node;
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        private static string ReadName(ReadOnlySpan<byte> SearchList, int index, ref int elapsed, ref List<ushort> passedPtr)
        {
            /* 
             * We will record all the pointers passed, if we encounter a pointer that we have passed before, it means we have a loop in the option value.
             * This detection method makes us still support forward pointers.
             */
            var str = string.Empty;
            byte len = SearchList[index];
            if (len > LABEL_LEN_MAX)
            {
                var ptr = (ushort)(BinaryPrimitives.ReadUInt16BigEndian(SearchList.Slice(index, 2)) & ~POINTER_MASK);
                if (passedPtr.Contains(ptr))
                {
                    throw new Exception("Loop detected in the option value.");
                }
                passedPtr.Add(ptr);
                elapsed += 2;
                int _ = 0;
                return ReadName(SearchList, ptr, ref _, ref passedPtr);
            }
            else if (len == 0)
            {
                passedPtr.Clear();
                elapsed += 1;
                return str;
            }
            else
            {
                elapsed += len + 1;
                str += System.Text.Encoding.ASCII.GetString(SearchList.Slice(index + 1, len).ToArray());
                return str + '.' + ReadName(SearchList, index + len + 1, ref elapsed, ref passedPtr);
            }
        }
    }
}
