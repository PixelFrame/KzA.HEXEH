using KzA.HEXEH.Core.Output;
using Serilog;
using System.Buffers.Binary;
using System.Text.Json;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class LengthedObjectParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;
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
        private bool isSchema = false;
        private IParser? nextParser;

        public override Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfLen", typeof(int) },
                { "ObjectParser", typeof(string) },
                { "IncludeSchema?", typeof(bool) },
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
            Log.Debug("[LengthedObjectParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (nextParser == null) { throw new InvalidOperationException("ObjectType not set"); }
                int len = 0;
                switch (lenOfLen)
                {
                    case 1: len = Input[Offset]; break;
                    case 2: len = BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                    case 4: len = BinaryPrimitives.ReadInt32LittleEndian(Input.Slice(Offset, 4)); break;
                }
                var children = nextParser.Parse(Input, Offset + lenOfLen, len, ParseStack);
                var head = new DataNode()
                {
                    Label = "Object with length specified",
                    Index = Offset,
                };
                head.Children.Add(new DataNode("Length", len.ToString(), Offset, lenOfLen));
                Read = len + lenOfLen;
                head.Children.Add(children);
                head.Length = Read;
                Log.Debug("[LengthedObjectParser] Parsed {Read} bytes", Read);
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
                Log.Error("[LengthedObjectParser] Actual object length exceeding given length");
                ParseStack!.Push(GetType().FullName ?? GetType().Name);
                throw new ParseLengthMismatchException("Actual object length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("IncludeSchema", out var isSchemaObj))
            {
                if (isSchemaObj is bool _isSchema)
                {
                    isSchema = _isSchema;
                }
                else
                {
                    throw new ArgumentException("Invalid Option: IncludeSchema");
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
            if (Options.TryGetValue("IncludeSchema", out var isSchemaStr))
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
