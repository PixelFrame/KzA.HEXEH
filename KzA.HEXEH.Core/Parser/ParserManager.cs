using KzA.HEXEH.Core.Schema;
using Serilog;

namespace KzA.HEXEH.Core.Parser
{
    public static class ParserManager
    {
        static ParserManager()
        {
            if (!Global.IsInitialized) { Global.Initialize(); }
            Log.Information("[ParserManager] Initialized");
        }

        private static IEnumerable<Type> availableParsers = [];
        public static IEnumerable<Type> AvailableParsers
        {
            get => availableParsers;
        }

        private static Dictionary<string, SchemaJsonObject> createdSchema = [];
        internal static Dictionary<string, SchemaJsonObject> CreatedSchema
        {
            get => createdSchema;
        }

        private static Dictionary<string, Type> createdEnums = [];
        internal static Dictionary<string, Type> CreatedEnums
        {
            get => createdEnums;
        }

        internal static void RefreshParsers()
        {
            Log.Information("[ParserManager] Refreshing available parsers");
            var desiredType = typeof(IParser);
            availableParsers = AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .Where(a => a.FullName!.StartsWith("KzA.HEXEH.Core"))
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(t => desiredType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            Log.Information("[ParserManager] Parsers refresh completed");
        }

        public static Type FindParserByBaseName(string Name)
        {
            Log.Information($"[ParserManager] Finding parser with base name {Name}");
            return AvailableParsers.Where(p => p.Name == (Name + "Parser")).FirstOrDefault() ??
                throw new ParserFindException($"{Name}Parser cannot be found");
        }

        public static Type FindParserByRelativeName(string Name, bool IsSchema)
        {
            var prefix = IsSchema ? "KzA.HEXEH.Core.Dynamic.Parser" : "KzA.HEXEH.Core.Parser";
            Log.Information($"[ParserManager] Finding parser with full name {prefix}.{Name}Parser");
            return AvailableParsers.Where(p => p.FullName == ($"{prefix}.{Name}Parser")).FirstOrDefault() ??
                throw new ParserFindException($"{prefix}.{Name}Parser cannot be found");
        }

        public static IParser InstantiateParserByBaseName(string Name, Dictionary<string, object>? Options = null)
        {
            var t = FindParserByBaseName(Name);
            Log.Information($"[ParserManager] Creating parser instance of {t.FullName}");
            var p = Activator.CreateInstance(t) as IParser ??
                throw new ParserFindException($"Cannot create instance of {t.FullName}");
            if (Options != null)
            {
                Log.Information($"[ParserManager] Setting options for parser instance of {t.FullName}");
                p.SetOptions(Options);
            }
            return p;
        }

        public static IParser InstantiateParserByRelativeName(string Name, bool IsSchema, Dictionary<string, object>? Options = null)
        {
            var t = FindParserByRelativeName(Name, IsSchema);
            Log.Information($"[ParserManager] Creating parser instance of {t.FullName}");
            var p = Activator.CreateInstance(t) as IParser ??
                throw new ParserFindException($"Cannot create instance of {t.FullName}");
            if (Options != null)
            {
                Log.Information($"[ParserManager] Setting options for parser instance of {t.FullName}");
                p.SetOptions(Options);
            }
            return p;
        }
    }

    public class ParserFindException(string message) : Exception(message)
    {
    }
}
