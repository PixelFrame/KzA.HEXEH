using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace KzA.HEXEH.Ext.Pwsh
{
    internal static class PSCommandExtension
    {
        public static string ToStringWithParam(this Command command)
        {
            var sb = new StringBuilder();
            sb.Append(command.CommandText);
            foreach (var param in command.Parameters)
            {
                sb.Append(' ');
                if (string.IsNullOrEmpty(param.Name))
                    sb.Append(param.Value);
                else
                    sb.Append($"-{param.Name} {param.Value}");
            }
            return sb.ToString();
        }
    }
}
