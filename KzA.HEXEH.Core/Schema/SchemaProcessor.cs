using KzA.HEXEH.Core.Parser;
using Serilog;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace KzA.HEXEH.Core.Schema
{
    internal static class SchemaProcessor
    {
        internal static SchemaJsonObject LoadSchema(string schemaJson)
        {
            Log.Information("Loading schema {schemaJson}", schemaJson);
            var schemaJsonContent = File.ReadAllText($"./Schema/{schemaJson}.json");
            Log.Debug($"Schema content:{Environment.NewLine}{{schemaJsonContent}}", schemaJsonContent);
            var schema = JsonSerializer.Deserialize<SchemaJsonObject>(schemaJsonContent)!;
            Log.Information("Loaded schema {schemaJson}", schemaJson);
            return schema;
        }

        internal static IEnumerable<Type> CreateEnums(SchemaJsonObject schemaObj)
        {
            var types = new List<Type>();

            if (schemaObj.Enums != null)
            {
                foreach (var enumObj in schemaObj.Enums)
                {
                    Log.Verbose("Creating Enum type {enumName}", enumObj.Name);
                    var eb = Global.DynamicModule.DefineEnum($"KzA.HEXEH.Core.Dynamic.Enums.{enumObj.Name}", TypeAttributes.Public, typeof(int));
                    foreach (var item in enumObj.Definition)
                    {
                        eb.DefineLiteral(item.Value, int.Parse(item.Key));
                    }
                    var enumType = eb.CreateType();
                    Log.Verbose("Created Enum type {enumName}", enumObj.Name);
                    types.Add(enumType);
                }
            }

            if (schemaObj.Flags != null)
            {
                foreach (var flagObj in schemaObj.Flags)
                {
                    Log.Verbose("Creating Flag type {enumName}", flagObj.Name);
                    var eb = Global.DynamicModule.DefineEnum($"KzA.HEXEH.Core.Dynamic.Enums.{flagObj.Name}", TypeAttributes.Public, typeof(int));
                    var flagsAttrib = new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(Type.EmptyTypes)!, Array.Empty<object>());
                    eb.SetCustomAttribute(flagsAttrib);
                    foreach (var item in flagObj.Definition)
                    {
                        eb.DefineLiteral(item.Value, int.Parse(item.Key));
                    }
                    var flagsType = eb.CreateType();
                    Log.Verbose("Created Flag type {enumName}", flagObj.Name);
                    types.Add(flagsType);
                }
            }
            return types;
        }

        internal static void InitializeSchemaParsers()
        {
            var dir = new DirectoryInfo("./Schema");
            var schemaFiles = dir.EnumerateFiles("*.json", SearchOption.AllDirectories);
            foreach (var schemaFile in schemaFiles)
            {
                Log.Verbose("Creating Parser for schema {schemaFile}", schemaFile);
                var tb = Global.DynamicModule.DefineType(
                        $"KzA.HEXEH.Core.Dynamic.Parser.{Path.GetFileNameWithoutExtension(schemaFile.FullName)}Parser",
                        TypeAttributes.Public | TypeAttributes.Class,
                        typeof(SchemaParser));
                _ = tb.CreateType();
                Log.Verbose("Created Parser for schema {schemaFile}", schemaFile);
            }
            ParserFinder.RefreshParsers();
        }
    }
}
