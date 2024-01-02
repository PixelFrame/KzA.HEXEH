using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using Serilog;

namespace KzA.HEXEH.Ext.Pwsh
{
    public class PwshParser : ParserBase
    {
        public override ParserType Type => ParserType.Extension;
        private string script = string.Empty;

        public override Dictionary<string, Type> GetOptions()
        {
            return new()
            {
                { "Script", typeof(string) }
            };
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
            return Parse(Input, Offset, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[PwshParser] Start parsing at {Offset}", Offset);
            var result = PsScriptRunner.RunScriptForResult(script, Input[Offset..].ToArray(), Offset, out Read);
            Log.Debug("[PwshParser] Parsed {Read} bytes", Read);
            return result;
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            throw new NotImplementedException();
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("Script", out var scriptObj))
            {
                if (scriptObj is string scriptStr)
                {
                    script = scriptStr;
                    Log.Debug("[PwshParser] Set option Script to {script}", script);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: Script");
                }
            }
            else
            {
                throw new ArgumentException("Script not provided");
            }
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("Script", out var scriptStr))
            {
                script = scriptStr;
                Log.Debug("[PwshParser] Set option Script to {script}", script);
            }
            else
            {
                throw new ArgumentException("Script not provided");
            }
        }
    }
}
