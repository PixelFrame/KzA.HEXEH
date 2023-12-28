using KzA.HEXEH.Core.Output;

namespace KzA.HEXEH.Core.Parser
{
    public abstract class ParserBase : IParser
    {
        public abstract ParserType Type { get; }

        public abstract Dictionary<string, Type> GetOptions();

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null);

        public abstract void SetOptions(Dictionary<string, object> Options);

        public abstract void SetOptionsFromSchema(Dictionary<string, string> Options);

        protected Stack<string> PrepareParseStack(Stack<string>? ParseStack)
        {
            ParseStack ??= new Stack<string>();
            ParseStack.Push(GetType().FullName ?? GetType().Name);
            return ParseStack;
        }
    }
}
