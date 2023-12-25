using KzA.HEXEH.Core.Output;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class NullTerminatedAsciiStringParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;
        private Encoding encoding = Encoding.ASCII;

        public Dictionary<string, Type> GetOptions()
        {
            return new();
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, Input.Length);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            return Parse(Input, 0, out Read);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            return Parse(Input, Offset, Input.Length - Offset);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            byte[] terminator = [0x00];
            var len = Input.IndexOf(terminator);
            Read = len + 1;
            return new DataNode($"String ({encoding.EncodingName})", encoding.GetString(Input.Slice(Offset, len).ToArray()));
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            var res = Parse(in Input, Offset, out int read);
            if (read != Length)
            {
                throw new ArgumentException("Given length does not match actual string length");
            }
            return res;
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotSupportedException();
        }
    }
}
