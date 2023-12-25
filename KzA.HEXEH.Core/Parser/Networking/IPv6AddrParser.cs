using KzA.HEXEH.Core.DataStructure.Networking;
using KzA.HEXEH.Core.Output;
using System.Runtime.InteropServices;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class IPv6AddrParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, 0, 16);
        }
        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            Read = 16;
            return Parse(Input, 0, 16);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            return Parse(Input, Offset, 16);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            Read = 16;
            return Parse(Input, Offset, 16);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
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
            return head;
        }

        public Dictionary<string, Type> GetOptions()
        {
            return new();
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotSupportedException();
        }

        public void SetSchema(string Schema)
        {
            throw new NotSupportedException();
        }
    }
}
