using Serilog;
using System.Text;

namespace KzA.HEXEH.Core.Utility
{
    internal static class StackExtension
    {
        internal static string Dump<T>(this Stack<T>? Stack)
        {
            if (Stack == null) return string.Empty;
            var sb = new StringBuilder();
            var depth = 0;
            var intend = "  ";
            while (Stack.Count > 0)
            {
                sb.Append(string.Concat(Enumerable.Repeat(intend, depth++)));
                sb.AppendLine(Stack.Pop()?.ToString());
            }
            return sb.ToString();
        }

        internal static T PopEx<T>(this Stack<T> Stack)
        {
            Log.Verbose($"Current stack:{Environment.NewLine}{{Stack}}", (new Stack<T>(Stack.ToArray())).Dump());
            return Stack.Pop();
        }
    }
}
