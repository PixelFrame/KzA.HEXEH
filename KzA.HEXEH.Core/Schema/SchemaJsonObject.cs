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
            NextParserExtension = 20,
        }

        // General
        public JsonParserType Type { get; set; }
        public string Target { get; set; } = string.Empty;
        public Dictionary<string, string>? Options { get; set; }
        public int Length { get; set; } = -1;
        public string LengthFromProp { get; set; } = string.Empty;
        public bool LengthFromParent { get; set; } = false;
        public int LengthModifier { get; set; } = 0;

        // BasicConvert
        public ValueConversion? Conversion { get; set; } = new();
        public bool BigEndian { get; set; } = false;

        // Schema
        public bool AllowFallback { get; set; } = false;
        public string FallbackTarget { get; set; } = "RAW";

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
