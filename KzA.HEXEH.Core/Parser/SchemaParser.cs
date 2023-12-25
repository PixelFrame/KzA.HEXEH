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
            var schemaName = typeName[..typeName.LastIndexOf("Parser")];
            _schema = SchemaProcessor.LoadSchema(schemaName);
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
                        node.Value = ParseBasic(def.Parser.Target, Input, ref Index, def.Parser.BigEndian); break;
                    case JsonParser.JsonParserType.BasicConvert:
                        if (def.Parser.Conversion == null) throw new SchemaException($"No conversion provided", _schema.Name, field);
                        node.Value = ParseBasicConvert(def.Parser.Target, Input, ref Index, def.Parser.Conversion, def.Parser.BigEndian); break;
                    case JsonParser.JsonParserType.NextParser:
                        var nextParser = ParserFinder.InstantiateParserByName(def.Parser.Target);
                        Log.Debug("Calling parser {nextParser} from {parentParser}", nextParser.GetType().FullName, this.GetType().FullName);
                        if (def.ParserConfig != null)
                        {
                            nextParser.SetOptions(def.ParserConfig);
                        }
                        var parsed = nextParser.Parse(in Input, Index, out Read);
                        node.Value = parsed.Value;
                        node.Detail = parsed.Detail;
                        node.Children.AddRange(parsed.Children);
                        break;
                    case JsonParser.JsonParserType.NextParserConditional:
                        var nextParserName = def.Parser.Target;
                        try
                        {
                            nextParserName = ParserPropRegex().Replace(nextParserName, m => head.Children.Where(n => n.Label == m.Groups[1].Value).First().Value);
                        }
                        catch
                        {
                            throw new SchemaException($"Unable to create conditional parser {nextParserName}", _schema.Name, field);
                        }
                        nextParser = ParserFinder.InstantiateParserByName(nextParserName);
                        Log.Debug("Calling parser {nextParser} from {parentParser}", nextParser.GetType().FullName, this.GetType().FullName);
                        if (def.ParserConfig != null)
                        {
                            nextParser.SetOptions(def.ParserConfig);
                        }
                        parsed = nextParser.Parse(in Input, Index, out Read);
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
            throw new NotImplementedException();
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

        internal string ParseBasic(string TypeName, in ReadOnlySpan<byte> Input, ref int Index, bool BigEndian)
        {
            switch (TypeName)
            {
                case "BYTE":
                    return Input[Index++].ToString();
                case "WORD":
                    var word = BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(Input.Slice(Index, 2)) : BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Index, 2));
                    Index += 2;
                    return $"{word} (0x{word:X})";
                case "DWORD":
                    var dword = BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(Input.Slice(Index, 4)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Index, 4));
                    Index += 4;
                    return $"{dword} (0x{dword:X})";
                case "QWORD":
                    var qword = BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(Input.Slice(Index, 8)) : BinaryPrimitives.ReadUInt64LittleEndian(Input.Slice(Index, 8));
                    Index += 8;
                    return $"{qword} (0x{qword:X})";
            }
            throw new SchemaException("No such type", _schema.Name, "");
        }

        internal string ParseBasicConvert(string TypeName, in ReadOnlySpan<byte> Input, ref int Index, ValueConversion Conversion, bool BigEndian)
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
            return Enum.ToObject(targetType, value).ToString() ??
                $"{value} (0x{value:X})";
        }

        [GeneratedRegex(@"{(\w+)}")]
        private static partial Regex ParserPropRegex();
    }
}
