using Serilog;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("KzA.HEXEH.Test")]

namespace KzA.HEXEH.Core.Utility
{
    public static class PsScriptRunner
    {
        const string PsFuncHeader =
@"function PsScriptConvert
{
    param (
        [Parameter()][byte[]] $value
    )
    ";
        const string PsFuncFooter =
@"
}";

        internal static string RunScriptForStringResult(string Script, byte[] Value)
        {
            var ps = PowerShell.Create();
            var rs = RunspaceFactory.CreateRunspace();
            ps.Runspace = rs;
            rs.Open();
            var funcScript = PsFuncHeader + Script + PsFuncFooter;
            Log.Debug($"[PsScriptRunner] PsScriptConvert function{Environment.NewLine}{{funcScript}}", funcScript);
            ps.AddScript(funcScript).AddStatement().AddCommand("PsScriptConvert").AddArgument(Value);
            var result = ps.Invoke<string>();
            if (ps.HadErrors)
            {
                var psErr = new StringBuilder();
                foreach (var error in ps.Streams.Error.ReadAll())
                {
                    psErr.AppendLine(error.ToString());
                }
                throw new Exception(psErr.ToString());
            }
            rs.Close();
            return string.Join(Environment.NewLine, result);
        }
    }
}
