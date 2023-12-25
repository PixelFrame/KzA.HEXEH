using KzA.HEXEH.Core.Output;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class IPv4AddrParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, 0, 4);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            Read = 4;
            return Parse(Input, 0, 4);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            return Parse(Input, Offset, 4);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            Read = 4;
            return Parse(Input, Offset, 4);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            if (Length != 4) { throw new ArgumentException("Length of IPv4 Address should be 4."); }
            if (Input.Length - Offset < 4) { throw new ArgumentException("Data too short."); }
            var head = new DataNode()
            {
                Label = "IPv4 Address",
                Value = $"{Input[Offset]}.{Input[Offset + 1]}.{Input[Offset + 2]}.{Input[Offset + 3]}",
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
