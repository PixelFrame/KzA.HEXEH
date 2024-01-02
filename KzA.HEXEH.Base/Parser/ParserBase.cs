using KzA.HEXEH.Base.Output;
using Serilog;

namespace KzA.HEXEH.Base.Parser
{
    public abstract class ParserBase : IParser
    {
        public abstract ParserType Type { get; }

        public bool BigEndian { get; set; } = false;

        public abstract Dictionary<string, Type> GetOptions();

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null);

        public abstract DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null);

        public virtual void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("BigEndian", out var endianObj))
            {
                if (endianObj is bool _bigEndian)
                {
                    BigEndian = _bigEndian;
                    Log.Debug("[{Caller}] Set option Encoding to {encoding}", GetType().Name, BigEndian);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: BigEndian");
                }
            }
        }

        public virtual void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("BigEndian", out var endianStr))
            {
                if (endianStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    BigEndian = true;
                    Log.Debug("[{Caller}] Set option Encoding to {encoding}", GetType().Name, BigEndian);
                }
            }
        }

        protected Stack<string> PrepareParseStack(Stack<string>? ParseStack)
        {
            ParseStack ??= new Stack<string>();
            ParseStack.Push(GetType().FullName ?? GetType().Name);
            return ParseStack;
        }
    }
}
