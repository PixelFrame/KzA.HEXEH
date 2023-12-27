using KzA.HEXEH.Core.Output;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Networking
{
    public class FqdnParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, 0, Input.Length);
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
            var start = Offset;
            var nullReached = false;
            var readLen = true;
            var len = 0;
            var sb = new StringBuilder();
            var loopCnt = 0;
            while (!nullReached)
            {
                if (readLen)
                {
                    readLen = false;
                    len = Input[Offset];
                    sb.Append($"({len})");
                    Offset++;
                    if (len == 0) { nullReached = true; }
                }
                else
                {
                    readLen = true;
                    sb.Append(Encoding.ASCII.GetString(Input.Slice(Offset, len).ToArray()));
                    Offset += len;
                }
                if (++loopCnt > Global.LoopMax)
                {
                    throw new StackOverflowException("Array loop exceeds limitation, please verify if data is valid or adjust the limitation");
                }
            }
            var result = new DataNode("FQDN", sb.ToString());
            Read = Offset - start;
            return result;
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            if (Length > 255) { throw new ArgumentException("FQDN data length must be less than 255"); }
            var res = Parse(in Input, Offset, out int read);
            if (read != Length)
            {
                throw new ArgumentException("Given length does not match actual FQDN length");
            }
            return res;
        }

        public Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotSupportedException();
        }

        public void SetSchema(string Schema)
        {
            throw new NotSupportedException();
        }

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotImplementedException();
        }
    }
}
