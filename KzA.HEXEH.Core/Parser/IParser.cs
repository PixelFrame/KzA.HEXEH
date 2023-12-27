using KzA.HEXEH.Core.Output;

namespace KzA.HEXEH.Core.Parser
{
    public interface IParser
    {
        public DataNode Parse(in ReadOnlySpan<byte> Input);
        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read);
        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset);
        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read);
        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length);
        public ParserType Type { get; }
        public Dictionary<string, Type> GetOptions();
        public void SetOptions(Dictionary<string, object> Options);
        public void SetOptionsFromSchema(Dictionary<string, string> Options);
    }

    public enum ParserType
    {
        Hardcoded,
        SchemaInternal,
        SchemaExternal
    }
}
