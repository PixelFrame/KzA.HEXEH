using KzA.HEXEH.Core.Output;
using System.Buffers.Binary;
using System.Text.Json;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class LengthedObjectArrayParser : IParser
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
        private int objectCount;
        private bool isSchema = false;
        private IParser? nextParser;

        public Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfLen", typeof(int) },
                { "ObjectCount", typeof(int) },
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

            var head = new DataNode()
            {
                Label = "Array of objects with length specified",
            };
            var start = Offset;
            int currentObjLen = 0;

            if (objectCount > 0)
            {
                for (var i = 0; i < objectCount; i++)
                {
                    switch (lenOfLen)
                    {
                        case 1: currentObjLen = Input[Offset]; break;
                        case 2: currentObjLen = BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                        case 4: currentObjLen = BinaryPrimitives.ReadInt32LittleEndian(Input.Slice(Offset, 4)); break;
                    }
                    Offset += lenOfLen;
                    head.Children.Add(new DataNode("Length", currentObjLen.ToString()));
                    head.Children.Add(nextParser.Parse(Input, Offset, currentObjLen));
                    Offset += currentObjLen;
                }
            }
            else
            {
                var loopCnt = 0;
                while (Offset < Input.Length)
                {
                    switch (lenOfLen)
                    {
                        case 1: currentObjLen = Input[Offset]; break;
                        case 2: currentObjLen = BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                        case 4: currentObjLen = BinaryPrimitives.ReadInt32LittleEndian(Input.Slice(Offset, 4)); break;
                    }
                    Offset += lenOfLen;
                    head.Children.Add(new DataNode("Length", currentObjLen.ToString()));
                    head.Children.Add(nextParser.Parse(Input, Offset, currentObjLen));
                    Offset += currentObjLen;
                    if (++loopCnt > Global.LoopMax)
                    {
                        throw new StackOverflowException("Array loop exceeds limitation, please verify if data is valid or adjust the limitation");
                    }
                }
            }
            Read = Offset - start;
            return head;
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            var res = Parse(in Input, Offset, out int read);
            if (read != Length)
            {
                throw new ArgumentException("Given length does not match actual object array length");
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

            if (Options.TryGetValue("ObjectCount", out var objectCountObj))
            {
                if (objectCountObj is int _objectCount) { objectCount = _objectCount; }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectCount");
                }
            }
            else
            {
                objectCount = 0;
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

            if (Options.TryGetValue("LenOfLen", out var lenOfLenStr))
            {
                if (int.TryParse(lenOfLenStr, out var __lenOfLen)) { lenOfLen = __lenOfLen; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfLen");
                }
            }
            else
            {
                throw new ArgumentException("LenOfLen not provided");
            }

            if (Options.TryGetValue("ObjectCount", out var objectCountStr))
            {
                if (int.TryParse(objectCountStr, out var _objectCount)) { objectCount = _objectCount; }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectCount");
                }
            }
            else
            {
                objectCount = 0;
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
