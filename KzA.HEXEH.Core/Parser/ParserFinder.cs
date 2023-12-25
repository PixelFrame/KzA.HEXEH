using Serilog;

namespace KzA.HEXEH.Core.Parser
{
    public static class ParserFinder
    {
        static ParserFinder()
        {
            if (!Global.IsInitialized) { Global.Initialize(); }
        }

        private static IEnumerable<Type> availableParsers = [];
        public static IEnumerable<Type> AvailableParsers
        {
            get => availableParsers;
        }

        internal static void RefreshParsers()
        {
            Log.Information("Refreshing available parsers");
            var desiredType = typeof(IParser);
            availableParsers = AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .Where(a => a.FullName!.StartsWith("KzA.HEXEH.Core"))
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(t => desiredType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            Log.Information("Parsers refresh completed");
        }

        public static Type FindParserByName(string Name)
        {
            Log.Information($"Finding parser of {Name}");
            return AvailableParsers.Where(p => p.Name == (Name + "Parser")).FirstOrDefault() ??
                throw new ParserFindException($"{Name}Parser cannot be found");
        }

        public static IParser InstantiateParserByName(string Name, Dictionary<string, object>? Options = null)
        {
            var t = FindParserByName(Name);
            Log.Information($"Creating parser instance of {Name}");
            var p = Activator.CreateInstance(t) as IParser ??
                throw new ParserFindException($"Cannot create instance of {Name}Parser");
            if (Options != null)
            {
                Log.Information($"Setting options for parser instance of {Name}");
                p.SetOptions(Options);
            }
            return p;
        }
    }

    public class ParserFindException(string message) : Exception(message)
    {
    }
}
