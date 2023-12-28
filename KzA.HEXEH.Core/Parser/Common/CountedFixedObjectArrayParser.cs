using KzA.HEXEH.Core.Output;
using Serilog;
using System.Buffers.Binary;
using System.Text.Json;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class CountedFixedObjectArrayParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;

        private int _lenOfCount;
        private int lenOfCount
        {
            get => _lenOfCount;
            set
            {
                if (!Global.ValidLengthNumberLen.Contains(value)) throw new ArgumentException("Invalid Option: LenOfLen should be one of the following values {1,2,4}");
                _lenOfCount = value;
            }
        }
        private int lenOfObject;
        private bool isSchema = false;
        private IParser? nextParser;

        public override Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfCount", typeof(int) },
                { "LenOfObject", typeof(int) },
                { "ObjectParser", typeof(string) },
                { "IsSchema?", typeof(bool) },
                { "ParserOptions?", typeof(Dictionary<string, object>) }
            };
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, Input.Length, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, Input.Length - Offset, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[CountedFixedObjectArrayParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
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
                    Label = "Array of fixed size objects",
                    Index = Offset
                };
                head.Children.Add(new DataNode("Count", count.ToString(), Offset, lenOfCount));
                head.Children.Add(new DataNode("Object Length", lenOfObject.ToString(), Offset, lenOfCount));
                Offset += lenOfCount;
                Read = (lenOfObject * count) + lenOfCount;
                for (; count > 0; count--)
                {
                    Log.Debug("[CountedFixedObjectArrayParser] Calling next parser {nextParserName}", nextParser.GetType().FullName);
                    head.Children.Add(nextParser.Parse(Input, Offset, lenOfObject, ParseStack));
                    Offset += lenOfObject;
                }

                head.Length = Read;
                Log.Debug("[CountedFixedObjectArrayParser] Parsed {Read} bytes", Read);
                ParseStack!.PopEx();
                return head;
            }
            catch (ParseException e)
            {
                throw new ParseFailureException("Failed to parse inner object", e.ParserStackPrint, Offset, e);
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse inner object", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            var res = Parse(in Input, Offset, out int read, ParseStack);
            if (read < Length)
            {
                var paddingNode = new DataNode()
                {
                    Label = "Padding (Unread Bytes)",
                    Value = BitConverter.ToString(Input.Slice(Offset + read, Length - read).ToArray()),
                    Index = Offset + read,
                    Length = Length - read,
                };
                res.Length = Length;
                res.Children.Add(paddingNode);
            }
            if (read > Length)
            {
                ParseStack!.Push(GetType().FullName ?? GetType().Name);
                throw new ParseLengthMismatchException("Actual object array length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
        }

        public override void SetOptions(Dictionary<string, object> Options)
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
                if (lenOfCountObj is int __lenOfCount) { lenOfCount = __lenOfCount; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfCount");
                }
            }
            else
            {
                throw new ArgumentException("LenOfCount not provided");
            }

            if (Options.TryGetValue("LenOfObject", out var lenOfObjectObj))
            {
                if (lenOfObjectObj is int _lenOfObject) { lenOfObject = _lenOfObject; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfObject");
                }
            }
            else
            {
                throw new ArgumentException("LenOfObject not provided");
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

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
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

            if (Options.TryGetValue("LenOfObject", out var lenOfObjectStr))
            {
                if (int.TryParse(lenOfObjectStr, out var _lenOfObject)) { lenOfObject = _lenOfObject; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfObject");
                }
            }
            else
            {
                throw new ArgumentException("LenOfObject not provided");
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
