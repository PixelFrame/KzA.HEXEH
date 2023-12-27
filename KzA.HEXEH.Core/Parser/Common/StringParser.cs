using KzA.HEXEH.Core.Output;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class StringParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;
        private Encoding encoding = Encoding.UTF8;

        public Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                {"Encoding", typeof(Encoding)},
            };
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, 0, Input.Length);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            throw new NotSupportedException("Unable to infer data length");
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            return Parse(Input, Offset, Input.Length - Offset);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            throw new NotSupportedException("Unable to infer data length");
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            return new DataNode($"String ({encoding.EncodingName})", encoding.GetString(Input.Slice(Offset, Length).ToArray()));
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                if (encodingObj is Encoding _encoding) { encoding = _encoding; }
                else
                {
                    throw new ArgumentException("Invalid Option: Encoding");
                }
            }
            else
            {
                throw new ArgumentException("Encoding not provided");
            }
        }

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                encoding = Encoding.GetEncoding(encodingObj);
            }
            else
            {
                throw new ArgumentException("Encoding not provided");
            }
        }
    }
}
