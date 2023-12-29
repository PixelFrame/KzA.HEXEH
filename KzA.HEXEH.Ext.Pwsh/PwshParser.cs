using KzA.HEXEH.Core.Output;
using KzA.HEXEH.Core.Parser;

namespace KzA.HEXEH.Ext.Pwsh
{
    internal class PwshParser : ParserBase
    {
        public override ParserType Type => throw new NotImplementedException();

        public override Dictionary<string, Type> GetOptions()
        {
            throw new NotImplementedException();
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            throw new NotImplementedException();
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            throw new NotImplementedException();
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            throw new NotImplementedException();
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            throw new NotImplementedException();
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            throw new NotImplementedException();
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotImplementedException();
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotImplementedException();
        }
    }
}
