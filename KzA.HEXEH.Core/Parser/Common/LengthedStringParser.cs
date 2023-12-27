using KzA.HEXEH.Core.Output;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class LengthedStringParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;
        private static readonly int[] validLen = { 1, 2, 4, 8 };
        private int _lenOfLen;
        private int lenOfLen
        {
            get => _lenOfLen;
            set
            {
                if (!validLen.Contains(value)) throw new ArgumentException("Invalid Option: LenOfLen should be one of the following values {1,2,4,8}");
                _lenOfLen = value;
            }
        }
        private Encoding encoding = Encoding.UTF8;

        public Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfLen", typeof(int) },
                { "Encoding", typeof(Encoding) },
            };
        }

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
            var innerParser = new LengthedObjectParser();
            var stringParserOpt = new Dictionary<string, object>()
            { {"Encoding", encoding } };
            innerParser.SetOptions(new Dictionary<string, object>()
            {
                {"LenOfLen", lenOfLen },
                {"ObjectParser", "Common.String" },
                {"ParserOptions", stringParserOpt },
            });
            var innerResult = innerParser.Parse(Input, Offset, out Read);
            innerResult.Label = "String with length specified";
            innerResult.Value = innerResult.Children[1].Value;
            return innerResult;
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            var innerParser = new LengthedObjectParser();
            var stringParserOpt = new Dictionary<string, object>()
            { {"Encoding", encoding } };
            innerParser.SetOptions(new Dictionary<string, object>()
            {
                {"LenOfLen", lenOfLen },
                {"ObjectParser", "Common.String" },
                {"ParserOptions", stringParserOpt },
            });
            var innerResult = innerParser.Parse(Input, Offset, Length);
            innerResult.Label = "String with length specified";
            innerResult.Value = innerResult.Children[1].Value;
            return innerResult;
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("LenOfLen", out var lenOfLenObj))
            {
                if (lenOfLenObj is int _lenOfLen) { lenOfLen = _lenOfLen; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfLen");
                }
            }
            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                if (encodingObj is Encoding _encoding) { encoding = _encoding; }
                else
                {
                    throw new ArgumentException("Invalid Option: Encoding");
                }
            }
        }

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("LenOfLen", out var lenOfLenObj))
            {
                if (int.TryParse(lenOfLenObj, out var _lenOfLen)) { lenOfLen = _lenOfLen; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfLen");
                }
            }

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
