using KzA.HEXEH.Core.Utility;
using KzA.HEXEH.Core.Schema;
using Serilog;
using System.Buffers.Binary;
using System.Text.RegularExpressions;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Base.Output;

namespace KzA.HEXEH.Core.Parser
{
    public abstract partial class SchemaParser : ParserBase
    {
        public override ParserType Type => ParserType.Schema;
        private SchemaJsonObject _schema;
        private IEnumerable<Type> _dynamicEnums;
        private readonly string[] ValidBaseTypes = ["BYTE", "WORD", "DWORD", "QWORD", "RAW"];
        private string _actualTypeName;
        private string _currentField = string.Empty;
        private Stack<string>? _currentStack = null;

        public SchemaParser()
        {
            _actualTypeName = GetType().FullName!;
            Log.Debug("[{_actualTypeName}] Creating Schema Parser Instance {_actualTypeName}", _actualTypeName);
            _schema = SchemaProcessor.LoadSchema(GetType());
            _dynamicEnums = SchemaProcessor.CreateEnums(_schema);
        }

        public override Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[{_actualTypeName}] Start parsing from {Offset}", _actualTypeName, Offset);
            ParseStack = PrepareParseStack(ParseStack);
            _currentStack = ParseStack;
            try
            {
                var index = Offset;
                var head = new DataNode()
                {
                    Label = _schema.Name,
                };
                var fieldNames = _schema.Structure.Fields.Split(":");
                foreach (var field in fieldNames)
                {
                    _currentField = field;
                    var def = _schema.Structure.Definition.Where(d => d.Name == field).FirstOrDefault() ??
                        throw new SchemaException("Missing Field Definition", _schema.Name, field);
                    var node = new DataNode()
                    {
                        Label = field,
                        Index = index,
                    };
                    switch (def.Parser.Type)
                    {
                        case JsonParser.JsonParserType.Basic:
                            ParseBasic(node, def.Parser.Target, Input, ref index, def.Parser.BigEndian, def.Parser.Length, def.Expected); break;
                        case JsonParser.JsonParserType.BasicConvert:
                            if (def.Parser.Conversion == null) throw new SchemaException($"No conversion provided", _schema.Name, field);
                            ParseBasicConvert(node, def.Parser.Target, Input, ref index, def.Parser.Conversion, def.Parser.BigEndian); break;
                        case JsonParser.JsonParserType.NextParser:
                            var nextParserName = def.Parser.Target;
                            try
                            {
                                nextParserName = ParserConditionRegex().Replace(nextParserName, m => ParserConditionReplacement(m, head.Children));
                            }
                            catch (Exception e)
                            {
                                throw new SchemaException($"Unable to create conditional parser {nextParserName}", _schema.Name, field, e);
                            }
                            try
                            {
                                nextParserName = ParserInterpolationRegex().Replace(nextParserName, m => ParserInterpolationReplacement(m, head.Children));
                            }
                            catch (Exception e)
                            {
                                throw new SchemaException($"Unable to create interpolation parser {nextParserName}", _schema.Name, field, e);
                            }
                            var nextParser = ParserManager.InstantiateParserByRelativeName(nextParserName, true);
                            Log.Debug("[{_actualTypeName}] Calling parser {nextParser}", _actualTypeName, nextParser.GetType().FullName);
                            if (def.Parser.Options != null)
                            {
                                var options = new Dictionary<string, string>(def.Parser.Options);
                                ProcessOptionCondition(options, head.Children);
                                ProcessOptionInterpolation(options, head.Children);
                                nextParser.SetOptionsFromSchema(options);
                            }
                            var parsed = nextParser.Parse(in Input, index, out Read, ParseStack);
                            node.Value = parsed.Value;
                            node.Detail = parsed.Detail;
                            node.Children.AddRange(parsed.Children);
                            node.Length = Read;
                            index += Read;
                            break;
                        case JsonParser.JsonParserType.NextParserExtension:
                            nextParser = ParserManager.InstantiateParserByFullName($"{def.Parser.ExtensionNamespace}.{def.Parser.Target}");
                            Log.Debug("[{_actualTypeName}] Calling parser {nextParser}", _actualTypeName, nextParser.GetType().FullName);
                            if (def.Parser.Options != null)
                            {
                                var options = new Dictionary<string, string>(def.Parser.Options);
                                ProcessOptionCondition(options, head.Children);
                                ProcessOptionInterpolation(options, head.Children);
                                nextParser.SetOptionsFromSchema(options);
                            }
                            parsed = nextParser.Parse(in Input, index, out Read, ParseStack);
                            node.Value = parsed.Value;
                            node.Detail = parsed.Detail;
                            node.Children.AddRange(parsed.Children);
                            node.Length = Read;
                            index += Read;
                            break;
                    }
                    head.Children.Add(node);
                }
                Read = index - Offset;
                head.Index = Offset;
                head.Length = Read;
                Log.Debug("[{_actualTypeName}] Parsed {Read} bytes", _actualTypeName, Read);
                ParseStack!.PopEx();
                return head;
            }
            catch (ParseException e)
            {
                throw new ParseFailureException("Failed to parse inner object", e.ParserStackPrint, Offset, e);
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            var res = Parse(Input, Offset, out var read, ParseStack);
            if (read < Length)
            {
                var paddingNode = new DataNode()
                {
                    Label = "Padding (Unread Bytes)",
                    Value = BitConverter.ToString(Input.Slice(Offset + read, Length - read).ToArray()),
                    Index = Offset + read,
                    Length = Length - read,
                };
                res.Children.Add(paddingNode);
            }
            if (read > Length)
            {
                Log.Error("[{_actualTypeName}] Actual object length exceeding given length", _actualTypeName);
                ParseStack!.Push(_actualTypeName);
                throw new ParseLengthMismatchException("Actual object length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotSupportedException();
        }

        private void SetSchema(SchemaJsonObject SchemaObj, IEnumerable<Type> DynamicEnums)
        {
            _schema = SchemaObj;
            _dynamicEnums = DynamicEnums;
        }

        private void ParseBasic(DataNode Node, string TypeName, in ReadOnlySpan<byte> Input, ref int Index, bool BigEndian, int Length, ulong? Expected)
        {
            switch (TypeName)
            {
                case "RAW":
                    Node.Value = BitConverter.ToString(Input.Slice(Index, Length).ToArray());
                    Node.Length = Length;
                    Index += Length;
                    return;
                case "BYTE":
                    Node.Value = Input[Index].ToString();
                    Node.DisplayValue = $"{Input[Index]} (0x{Input[Index]:X})";
                    if (Expected != null && !Expected.Equals((ulong)Input[Index]))
                    {
                        throw new ParseUnexpectedValueException($"{_currentField} value {Input[Index]} does not equal to expected value {Expected}", _currentStack.Dump(), Index);
                    }
                    Node.Length = 1;
                    Index++;
                    return;
                case "WORD":
                    var word = BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(Input.Slice(Index, 2)) : BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Index, 2));
                    Node.Value = word.ToString();
                    Node.DisplayValue = $"{word} (0x{word:X})";
                    if (Expected != null && !Expected.Equals((ulong)word))
                    {
                        throw new ParseUnexpectedValueException($"{_currentField} value {word} does not equal to expected value {Expected}", _currentStack.Dump(), Index);
                    }
                    Node.Length = 2;
                    Index += 2;
                    return;
                case "DWORD":
                    var dword = BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(Input.Slice(Index, 4)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Index, 4));
                    Node.Value = dword.ToString();
                    Node.DisplayValue = $"{dword} (0x{dword:X})";
                    if (Expected != null && !Expected.Equals((ulong)dword))
                    {
                        throw new ParseUnexpectedValueException($"{_currentField} value {dword} does not equal to expected value {Expected}", _currentStack.Dump(), Index);
                    }
                    Node.Length = 4;
                    Index += 4;
                    return;
                case "QWORD":
                    var qword = BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(Input.Slice(Index, 8)) : BinaryPrimitives.ReadUInt64LittleEndian(Input.Slice(Index, 8));
                    Node.Value = qword.ToString();
                    Node.DisplayValue = $"{qword} (0x{qword:X})";
                    if (Expected != null && !Expected.Equals(qword))
                    {
                        throw new ParseUnexpectedValueException($"{_currentField} value {qword} does not equal to expected value {Expected}", _currentStack.Dump(), Index);
                    }
                    Node.Length = 8;
                    Index += 8;
                    return;
            }
            throw new SchemaException($"No such type {TypeName}", _schema.Name, "");
        }

        private void ParseBasicConvert(DataNode Node, string TypeName, in ReadOnlySpan<byte> Input, ref int Index, ValueConversion Conversion, bool BigEndian)
        {
            ulong value = 0;
            switch (TypeName)
            {
                case "BYTE":
                    value = Input[Index++];
                    Node.Length = 1;
                    break;
                case "WORD":
                    value = BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(Input.Slice(Index, 2)) : BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Index, 2));
                    Node.Length = 2;
                    Index += 2;
                    break;
                case "DWORD":
                    value = BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(Input.Slice(Index, 4)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Index, 4));
                    Node.Length = 4;
                    Index += 4;
                    break;
                case "QWORD":
                    value = BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(Input.Slice(Index, 8)) : BinaryPrimitives.ReadUInt64LittleEndian(Input.Slice(Index, 8));
                    Node.Length = 8;
                    Index += 8;
                    break;
            }
            var targetType = _dynamicEnums.Where(t => t.Name == Conversion.Target).FirstOrDefault() ??
                throw new SchemaException($"Target type \"{Conversion.Target}\" not defined", _schema.Name, "");
            Node.Value = Enum.ToObject(targetType, value).ToString() ??
                $"{value} (0x{value:X})";
        }

        private string ParserConditionReplacement(Match m, IEnumerable<DataNode> parsed)
        {
            var _key = m.Groups["key"].Value;
            var _cases = m.Groups["case"].Captures;
            var _values = m.Groups["value"].Captures;

            var parsedValue = parsed.Where(n => n.Label == _key).First().Value;
            for (int i = 0; i < _cases.Count; ++i)
            {
                if (_cases[i].Value == parsedValue || _cases[i].Value == string.Empty)
                    return _values[i].Value;
            }
            throw new SchemaException($"Condition not covered", _schema.Name, "");
        }

        private string ParserInterpolationReplacement(Match m, IEnumerable<DataNode> parsed)
        {
            return parsed.Where(n => n.Label == m.Groups[1].Value).First().Value;
        }

        private void ProcessOptionCondition(Dictionary<string, string> original, IEnumerable<DataNode> parsed)
        {
            foreach (var key in original.Keys)
            {
                try
                {
                    original[key] = ParserConditionRegex().Replace(original[key], m => ParserConditionReplacement(m, parsed));
                }
                catch (Exception e)
                {
                    throw new SchemaException("Unable to complete option conditional replacement", _schema.Name, $"{_currentField}.{key}", e);
                }
            }
        }

        private void ProcessOptionInterpolation(Dictionary<string, string> original, IEnumerable<DataNode> parsed)
        {
            foreach (var key in original.Keys)
            {
                try
                {
                    original[key] = ParserInterpolationRegex().Replace(original[key], m => ParserInterpolationReplacement(m, parsed));
                }
                catch (Exception e)
                {
                    throw new SchemaException("Unable to complete option interpolation replacement", _schema.Name, $"{_currentField}.{key}", e);
                }
            }
        }

        [GeneratedRegex(@"{(\w+)}")]
        private static partial Regex ParserInterpolationRegex();

        [GeneratedRegex(@"\[(?<key>\w+);((?<case>.*?):(?<value>[\w\.]+);?)+\]")]
        private static partial Regex ParserConditionRegex();

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotSupportedException();
        }
    }
}
