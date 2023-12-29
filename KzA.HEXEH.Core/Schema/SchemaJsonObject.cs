namespace KzA.HEXEH.Core.Schema
{
    internal class SchemaJsonObject
    {
        public string Name { get; set; } = "NewSchema";
        public Structure Structure { get; set; } = new();
        public EnumDef[]? Enums { get; set; } = null;
        public FlagDef[]? Flags { get; set; } = null;
    }

    internal class Structure
    {
        public string Fields { get; set; } = string.Empty;
        public StructureDef[] Definition { get; set; } = [];
    }

    internal class StructureDef
    {
        public string Name { get; set; } = string.Empty;
        public JsonParser Parser { get; set; } = new();
        public ulong? Expected { get; set; } = null;
    }

    internal class JsonParser
    {
        internal enum JsonParserType
        {
            Basic = 0,
            BasicConvert = 1,
            NextParser = 10,
            NextParserExtension = 15,
        }

        // General
        public JsonParserType Type { get; set; }
        public string Target { get; set; } = string.Empty;
        public Dictionary<string, string>? Options { get; set; }

        // BasicConvert
        public ValueConversion? Conversion { get; set; } = new();
        public bool BigEndian { get; set; } = false;
        public int Length { get; set; } = -1;

        // Extension
        public string ExtensionNamespace { get; set; } = string.Empty;
    }

    internal class ValueConversion
    {
        internal enum ConversionType
        {
            Enum = 1,
            Flag = 2,
        }
        public ConversionType Type { get; set; } = ConversionType.Enum;
        public string Target { get; set; } = string.Empty;
    }

    internal class EnumDef
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Definition { get; set; } = new();
    }

    internal class FlagDef
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Definition { get; set; } = new();
    }
}
