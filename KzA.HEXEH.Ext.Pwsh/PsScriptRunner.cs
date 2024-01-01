using KzA.HEXEH.Base.Output;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace KzA.HEXEH.Ext.Pwsh
{
    public static class PsScriptRunner
    {
        private static readonly string BASE_DLL;
        static PsScriptRunner()
        {
            BASE_DLL = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName?.StartsWith("KzA.HEXEH.Base") ?? false).First().Location;
        }

        internal static DataNode RunScriptForResult(string Script, byte[] Value, int Offset, out int Read)
        {
            var ps = PowerShell.Create();
            var rs = RunspaceFactory.CreateRunspace();
            ps.Runspace = rs;
            rs.Open();
            ps.AddCommand("Add-Type").AddParameter("Path", BASE_DLL)
                .AddStatement().AddScript("using namespace KzA.HEXEH.Base.Output")
                .AddStatement().AddScript(Script)
                .AddStatement().AddCommand("Parse").AddArgument(Value).AddArgument(Offset);
            var result = ps.Invoke<object>();
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
            Read = (int)result[0];
            var dn = result[1] as DataNode ??
                throw new Exception("PowerShell returned invalid result");
            return dn;
        }
    }
}
