using KzA.HEXEH.Core.Output;
using System.Buffers.Binary;
using System.Text.Json;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class CountedLengthInheritedObjectArrayParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;
        private int lenOfCount;
        private bool isSchema = false;
        private IParser? nextParser;

        public Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfCount", typeof(int) },
                { "ObjectParser", typeof(string) },
                { "IsSchema?", typeof(bool) },
                { "ParserOptions?", typeof(Dictionary<string, object>) }
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
            int count = 0;
            switch (lenOfCount)
            {
                case 1: count = Input[Offset]; break;
                case 2: count = BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                case 4: count = BinaryPrimitives.ReadInt32LittleEndian(Input.Slice(Offset, 4)); break;
            }

            var head = new DataNode()
            {
                Label = "Array of objects with length inherited and count specified",
            };
            var start = Offset;
            Offset += lenOfCount;
            for (var i = 0; i < count; i++)
            {
                head.Children.Add(nextParser.Parse(Input, Offset, out int currentObjLen));
                Offset += currentObjLen;
            }

            Read = Offset - start;
            return head;
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            var res = Parse(in Input, Offset, out int read);
            if (read < Length)
            {
                var paddingNode = new DataNode()
                {
                    Label = "Padding (Unread Bytes)",
                    Value = BitConverter.ToString(Input.Slice(Offset + read, Length - read).ToArray()),
                };
                res.Children.Add(paddingNode);
            }
            if (read > Length)
            {
                throw new ArgumentException("Actual object array length exceeding given length");
            }
            return res;
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("IsSchema", out var isSchemaObj))
            {
                if (isSchemaObj is bool _isSchema)
                {
                    isSchema = _isSchema;
                }
                else
                {
                    throw new ArgumentException("Invalid Option: IsSchema");
                }
            }

            if (Options.TryGetValue("ObjectParser", out var targetTypeNameObj))
            {
                if (targetTypeNameObj is string targetTypeName)
                {
                    nextParser = ParserManager.InstantiateParserByRelativeName(targetTypeName, isSchema);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectParser");
                }
            }
            else
            {
                throw new ArgumentException("ObjectParser not provided");
            }

            if (Options.TryGetValue("LenOfCount", out var lenOfCountObj))
            {
                if (lenOfCountObj is int _lenOfCount) { lenOfCount = _lenOfCount; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfCount");
                }
            }
            else
            {
                throw new ArgumentException("LenOfCount not provided");
            }

            if (Options.TryGetValue("ParserOptions", out var nextParserOptionsObj))
            {
                if (nextParserOptionsObj is Dictionary<string, object> nextParserOptions)
                {
                    nextParser.SetOptions(nextParserOptions);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ParserOptions");
                }
            }
        }

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("IsSchema", out var isSchemaStr))
            {
                if (isSchemaStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    isSchema = true;
                }
            }

            if (Options.TryGetValue("ObjectParser", out var targetTypeName))
            {
                nextParser = ParserManager.InstantiateParserByRelativeName(targetTypeName, isSchema);
            }
            else
            {
                throw new ArgumentException("ObjectParser not provided");
            }

            if (Options.TryGetValue("LenOfCount", out var lenOfCountStr))
            {
                if (int.TryParse(lenOfCountStr, out var __lenOfCount)) { lenOfCount = __lenOfCount; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfCount");
                }
            }
            else
            {
                throw new ArgumentException("LenOfCount not provided");
            }

            if (Options.TryGetValue("ParserOptions", out var nextParserOptionsStr))
            {
                var nextParserOptions = JsonSerializer.Deserialize<Dictionary<string, string>>(nextParserOptionsStr);
                if (nextParserOptions != null)
                {
                    nextParser.SetOptionsFromSchema(nextParserOptions);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ParserOptions");
                }
            }
        }
    }
}
