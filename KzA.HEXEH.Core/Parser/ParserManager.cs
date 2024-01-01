using KzA.HEXEH.Base.Parser;
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

        private static IList<Type> availableParsers = [];
        public static IList<Type> AvailableParsers
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
                   .Where(t => desiredType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                   .ToList();
            Log.Information("[ParserManager] Parsers refresh completed");
        }

        public static Type FindParserByBaseName(string Name)
        {
            Log.Information($"[ParserManager] Finding parser with base name {Name}");
            var found = AvailableParsers.Where(p => p.Name == (Name + "Parser"));
            if (!found.Any())
                throw new ParserFindException($"{Name}Parser cannot be found");
            if (found.Count() > 1)
                throw new ParserFindException($"{Name}Parser is ambiguous");
            return found.First();
        }

        public static Type FindParserByRelativeName(string Name, bool IncludeSchema)
        {
            var prefix = "KzA.HEXEH.Core.Parser";
            Log.Information($"[ParserManager] Finding parser with full name {prefix}.{Name}Parser");
            var found = AvailableParsers.Where(p => p.FullName == ($"{prefix}.{Name}Parser")).FirstOrDefault();
            if (found == null && IncludeSchema)
            {
                prefix = "KzA.HEXEH.Core.Dynamic.Parser";
                Log.Information($"[ParserManager] Finding parser with full name {prefix}.{Name}Parser");
                found = AvailableParsers.Where(p => p.FullName == ($"{prefix}.{Name}Parser")).FirstOrDefault();
            }
            return found ??
                throw new ParserFindException($"Parser with relative name {Name} is not found, IncludeSchema={IncludeSchema}");
        }

        public static Type FindParserByFullName(string Name)
        {
            Log.Information($"[ParserManager] Finding parser with full name {Name}Parser");
            var found = AvailableParsers.Where(p => p.FullName == ($"{Name}Parser")).FirstOrDefault();
            return found ??
                throw new ParserFindException($"Parser with full name {Name} is not found");
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

        public static IParser InstantiateParserByRelativeName(string Name, bool IncludeSchema, Dictionary<string, object>? Options = null)
        {
            var t = FindParserByRelativeName(Name, IncludeSchema);
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

        public static IParser InstantiateParserByFullName(string Name, Dictionary<string, object>? Options = null)
        {
            var t = FindParserByFullName(Name);
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
