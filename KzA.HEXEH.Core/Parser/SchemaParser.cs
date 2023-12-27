using KzA.HEXEH.Core.Output;
using KzA.HEXEH.Core.Schema;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace KzA.HEXEH.Core.Parser
{
    public abstract partial class SchemaParser : IParser
    {
        public ParserType Type => ParserType.SchemaInternal;
        private SchemaJsonObject _schema;
        private IEnumerable<Type> _dynamicEnums;
        private readonly string[] ValidBaseTypes = ["BYTE", "WORD", "DWORD", "QWORD"];

        public SchemaParser()
        {
            var typeName = GetType().Name;
            Log.Debug("Creating Dynamic Parser Instance {typeName}", typeName);
            _schema = SchemaProcessor.LoadSchema(GetType());
            _dynamicEnums = SchemaProcessor.CreateEnums(_schema);
        }

        public Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, out _);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            return Parse(Input, 0, out Read);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            return Parse(Input, 0, out _);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            var Index = Offset;
            var head = new DataNode()
            {
                Label = _schema.Name,
            };
            var fieldNames = _schema.Structure.Fields.Split(":");
            foreach (var field in fieldNames)
            {
                var def = _schema.Structure.Definition.Where(d => d.Name == field).FirstOrDefault() ??
                    throw new SchemaException("Missing Field Definition", _schema.Name, field);
                var node = new DataNode()
                {
                    Label = field
                };
                switch (def.Parser.Type)
                {
                    case JsonParser.JsonParserType.Basic:
                        ParseBasic(ref node, def.Parser.Target, Input, ref Index, def.Parser.BigEndian); break;
                    case JsonParser.JsonParserType.BasicConvert:
                        if (def.Parser.Conversion == null) throw new SchemaException($"No conversion provided", _schema.Name, field);
                        ParseBasicConvert(ref node, def.Parser.Target, Input, ref Index, def.Parser.Conversion, def.Parser.BigEndian); break;
                    case JsonParser.JsonParserType.NextParserBuiltin:
                    case JsonParser.JsonParserType.NextParserSchema:
                        var nextParser = ParserManager.InstantiateParserByRelativeName(def.Parser.Target, def.Parser.Type == JsonParser.JsonParserType.NextParserSchema);
                        Log.Debug("Calling parser {nextParser} from {parentParser}", nextParser.GetType().FullName, this.GetType().FullName);
                        if (def.Parser.Options != null)
                        {
                            nextParser.SetOptionsFromSchema(def.Parser.Options);
                        }
                        var parsed = nextParser.Parse(in Input, Index, out Read);
                        Index += Read;
                        node.Value = parsed.Value;
                        node.Detail = parsed.Detail;
                        node.Children.AddRange(parsed.Children);
                        break;
                    case JsonParser.JsonParserType.NextParserBuiltinInterpolation:
                    case JsonParser.JsonParserType.NextParserSchemaInterpolation:
                        var nextParserName = def.Parser.Target;
                        try
                        {
                            nextParserName = ParserInterpolationRegex().Replace(nextParserName, m => head.Children.Where(n => n.Label == m.Groups[1].Value).First().Value);
                        }
                        catch (Exception e)
                        {
                            throw new SchemaException($"Unable to create conditional parser {nextParserName}", _schema.Name, field, e);
                        }
                        nextParser = ParserManager.InstantiateParserByRelativeName(nextParserName, def.Parser.Type == JsonParser.JsonParserType.NextParserSchemaInterpolation);
                        Log.Debug("Calling parser {nextParser} from {parentParser}", nextParser.GetType().FullName, this.GetType().FullName);
                        if (def.Parser.Options != null)
                        {
                            nextParser.SetOptionsFromSchema(def.Parser.Options);
                        }
                        parsed = nextParser.Parse(in Input, Index, out Read);
                        Index += Read;
                        node.Value = parsed.Value;
                        node.Detail = parsed.Detail;
                        node.Children.AddRange(parsed.Children);
                        break;
                    case JsonParser.JsonParserType.NextParserBuiltinCondition:
                    case JsonParser.JsonParserType.NextParserSchemaCondition:
                        nextParserName = def.Parser.Target;
                        try
                        {
                            nextParserName = ParserConditionRegex().Replace(nextParserName, m => ParserConditionReplacement(m, head.Children, field));
                        }
                        catch (Exception e)
                        {
                            throw new SchemaException($"Unable to create conditional parser {nextParserName}", _schema.Name, field, e);
                        }
                        nextParser = ParserManager.InstantiateParserByRelativeName(nextParserName, def.Parser.Type == JsonParser.JsonParserType.NextParserSchemaCondition);
                        Log.Debug("Calling parser {nextParser} from {parentParser}", nextParser.GetType().FullName, this.GetType().FullName);
                        if (def.Parser.Options != null)
                        {
                            nextParser.SetOptionsFromSchema(def.Parser.Options);
                        }
                        parsed = nextParser.Parse(in Input, Index, out Read);
                        Index += Read;
                        node.Value = parsed.Value;
                        node.Detail = parsed.Detail;
                        node.Children.AddRange(parsed.Children);
                        break;
                    case JsonParser.JsonParserType.PsScript:
                        node.Value = PsScriptRunner.RunScriptForStringResult(def.Parser.Target, Input.Slice(Index, def.Parser.Length).ToArray());
                        Index += def.Parser.Length;
                        break;
                }
                head.Children.Add(node);
            }
            Read = Index - Offset;
            return head;
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            var result = Parse(Input, Offset, out var Read);
            if (Length != Read)
                throw new SchemaException($"Read bytes count does not match given length", _schema.Name, "");
            return result;
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotImplementedException();
        }

        private void SetSchema(SchemaJsonObject SchemaObj, IEnumerable<Type> DynamicEnums)
        {
            _schema = SchemaObj;
            _dynamicEnums = DynamicEnums;
        }

        private void ParseBasic(ref DataNode Node, string TypeName, in ReadOnlySpan<byte> Input, ref int Index, bool BigEndian)
        {
            switch (TypeName)
            {
                case "BYTE":
                    Node.Value = Input[Index].ToString();
                    Node.DisplayValue = $"{Input[Index]} (0x{Input[Index]:X})";
                    Index++;
                    return;
                case "WORD":
                    var word = BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(Input.Slice(Index, 2)) : BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Index, 2));
                    Node.Value = word.ToString();
                    Node.DisplayValue = $"{word} (0x{word:X})"; 
                    Index += 2;
                    return;
                case "DWORD":
                    var dword = BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(Input.Slice(Index, 4)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Index, 4));
                    Node.Value = dword.ToString();
                    Node.DisplayValue = $"{dword} (0x{dword:X})";
                    Index += 4;
                    return;
                case "QWORD":
                    var qword = BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(Input.Slice(Index, 8)) : BinaryPrimitives.ReadUInt64LittleEndian(Input.Slice(Index, 8));
                    Node.Value = qword.ToString();
                    Node.DisplayValue = $"{qword} (0x{qword:X})";
                    Index += 8;
                    return;
            }
            throw new SchemaException($"No such type {TypeName}", _schema.Name, "");
        }

        private void ParseBasicConvert(ref DataNode Node, string TypeName, in ReadOnlySpan<byte> Input, ref int Index, ValueConversion Conversion, bool BigEndian)
        {
            ulong value = 0;
            switch (TypeName)
            {
                case "BYTE":
                    value = Input[Index++]; break;
                case "WORD":
                    value = BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(Input.Slice(Index, 2)) : BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Index, 2));
                    Index += 2;
                    break;
                case "DWORD":
                    value = BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(Input.Slice(Index, 4)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Index, 4));
                    Index += 4;
                    break;
                case "QWORD":
                    value = BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(Input.Slice(Index, 8)) : BinaryPrimitives.ReadUInt64LittleEndian(Input.Slice(Index, 8));
                    Index += 8;
                    break;
            }
            var targetType = _dynamicEnums.Where(t => t.Name == Conversion.Target).FirstOrDefault() ??
                throw new SchemaException($"Target type \"{Conversion.Target}\" not defined", _schema.Name, "");
            Node.Value = Enum.ToObject(targetType, value).ToString() ??
                $"{value} (0x{value:X})";
        }

        private string ParserConditionReplacement(Match m, IEnumerable<DataNode> parsed, string field)
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
            throw new SchemaException($"Condition not covered", _schema.Name, field);
        }

        [GeneratedRegex(@"{(\w+)}")]
        private static partial Regex ParserInterpolationRegex();

        [GeneratedRegex(@"\[(?<key>\w+);((?<case>.*?):(?<value>[\w\.]+);?)+\]")]
        private static partial Regex ParserConditionRegex();

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotImplementedException();
        }
    }
}
