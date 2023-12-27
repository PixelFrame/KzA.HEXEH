using KzA.HEXEH.Core.DataStructure.Networking;
using KzA.HEXEH.Core.Output;
using Serilog;
using System.Runtime.InteropServices;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class IPv6AddrParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;

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
            Log.Debug("[IPv6AddrParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (Length != 16) { throw new ArgumentException("Length of IPv6 Address should be 16."); }
                if (Input.Length - Offset < Length) { throw new ArgumentException("Data too short."); }

                var byteArr = Input.Slice(Offset, 16).ToArray();
                var handle = GCHandle.Alloc(byteArr, GCHandleType.Pinned);
                var addrObj = (IPv6Addr)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(IPv6Addr))!;
                handle.Free();

                var head = new DataNode()
                {
                    Label = "IPv6 Address",
                    Value = addrObj.ToString(),
                };
                Log.Debug("[IPv6AddrParser] Parsed 16 bytes");
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
