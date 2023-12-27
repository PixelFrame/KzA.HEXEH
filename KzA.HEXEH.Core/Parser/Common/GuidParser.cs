using KzA.HEXEH.Core.Output;

namespace KzA.HEXEH.Core.Parser.Common
{
    internal class GuidParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;

        public Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, 0);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            Read = 16;
            return Parse(Input);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            var guid = new Guid(Input.Slice(Offset, 16));
            return new DataNode()
            {
                Label = "GUID",
                Value = guid.ToString(),
            };
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            Read = 16;
            return Parse(Input, Offset);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            if (Length != 16) throw new ArgumentException("GUID length must be 16");
            return Parse(Input, Offset);
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotImplementedException();
        }

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotImplementedException();
        }
    }
}
