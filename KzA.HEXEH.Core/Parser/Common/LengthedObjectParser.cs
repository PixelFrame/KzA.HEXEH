using KzA.HEXEH.Core.Output;
using System.Buffers.Binary;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class LengthedObjectParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;
        private int _lenOfLen;
        private int lenOfLen
        {
            get => _lenOfLen;
            set
            {
                if (!Global.ValidLengthNumberLen.Contains(value)) throw new ArgumentException("Invalid Option: LenOfLen should be one of the following values {1,2,4}");
                _lenOfLen = value;
            }
        }
        private IParser? nextParser;

        public Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfLen", typeof(int) },
                { "ObjectType", typeof(string) },
                { "ObjectOptions?", typeof(Dictionary<string, object>) }
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
            if (nextParser == null) { throw new InvalidOperationException("ObjectType not set"); }
            int len = 0;
            switch (lenOfLen)
            {
                case 1: len = Input[Offset]; break;
                case 2: len = BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                case 4: len = BinaryPrimitives.ReadInt32LittleEndian(Input.Slice(Offset, 4)); break;
            }
            var children = nextParser.Parse(Input, Offset + lenOfLen, len);
            var head = new DataNode()
            {
                Label = "Object with length specified",
            };
            head.Children.Add(new DataNode("Length", len.ToString()));
            head.Children.Add(children);
            Read = len + lenOfLen;
            return head;
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            var res = Parse(in Input, Offset, out int read);
            if (read != Length)
            {
                throw new ArgumentException("Data length does not match");
            }
            return res;
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("ObjectType", out var targetTypeNameObj))
            {
                if (targetTypeNameObj is string targetTypeName)
                {
                    nextParser = ParserManager.InstantiateParserByBaseName(targetTypeName);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectType");
                }
            }
            else
            {
                throw new ArgumentException("ObjectType not provided");
            }

            if (Options.TryGetValue("LenOfLen", out var lenOfLenObj))
            {
                if (lenOfLenObj is int _lenOfLen) { lenOfLen = _lenOfLen; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfLen");
                }
            }
            else
            {
                throw new ArgumentException("LenOfLen not provided");
            }

            if (Options.TryGetValue("ObjectOptions?", out var nextParserOptionsObj))
            {
                if (nextParserOptionsObj is Dictionary<string, object> nextParserOptions)
                {
                    nextParser.SetOptions(nextParserOptions);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectOptions");
                }
            }
        }
    }
}
