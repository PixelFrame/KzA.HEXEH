using KzA.HEXEH.Core.Parser;
using Serilog;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KzA.HEXEH.Core.Schema
{
    internal static partial class SchemaProcessor
    {
        internal static SchemaJsonObject LoadSchema(Type ParserType)
        {
            if (!ParserType.IsAssignableTo(typeof(IParser)))
                throw new ArgumentException($"{ParserType.FullName} is not a schema parser");
            var schemaName = GenerateSchemaFilePath(ParserType);
            if (ParserManager.CreatedSchema.TryGetValue(schemaName, out SchemaJsonObject? value))
            {
                Log.Debug("[SchemaProcessor] SchemaJsonObject {schemaName} existing", schemaName);
                return value;
            }
            Log.Debug("[SchemaProcessor] Loading schema {schemaName}", schemaName);
            var schemaJsonContent = File.ReadAllText($"./Schema/{schemaName}");
            Log.Verbose($"[SchemaProcessor] Schema content:{Environment.NewLine}{{schemaJsonContent}}", schemaJsonContent);
            var schema = JsonSerializer.Deserialize<SchemaJsonObject>(schemaJsonContent)!;
            ParserManager.CreatedSchema.Add(schemaName, schema);
            Log.Debug("[SchemaProcessor] Loaded schema {schemaName}", schemaName);
            return schema;
        }

        internal static IEnumerable<Type> CreateEnums(SchemaJsonObject schemaObj)
        {
            var types = new List<Type>();

            if (schemaObj.Enums != null)
            {
                foreach (var enumObj in schemaObj.Enums)
                {
                    if (ParserManager.CreatedEnums.TryGetValue(enumObj.Name, out Type? value))
                    {
                        Log.Debug("[SchemaProcessor] Flag type {enumName} existing", enumObj.Name);
                        types.Add(value);
                        continue;
                    }
                    Log.Debug("[SchemaProcessor] Creating Enum type {enumName}", enumObj.Name);
                    var eb = Global.DynamicModule.DefineEnum($"KzA.HEXEH.Core.Dynamic.Enums.{enumObj.Name}", TypeAttributes.Public, typeof(int));
                    foreach (var item in enumObj.Definition)
                    {
                        eb.DefineLiteral(item.Value, int.Parse(item.Key));
                    }
                    var enumType = eb.CreateType();
                    Log.Debug("[SchemaProcessor] Created Enum type {enumName}", enumObj.Name);
                    types.Add(enumType);
                    ParserManager.CreatedEnums.Add(enumObj.Name, enumType);
                }
            }

            if (schemaObj.Flags != null)
            {
                foreach (var flagObj in schemaObj.Flags)
                {
                    if (ParserManager.CreatedEnums.TryGetValue(flagObj.Name, out Type? value))
                    {
                        Log.Debug("[SchemaProcessor] Flag type {enumName} existing", flagObj.Name);
                        types.Add(value);
                        continue;
                    }
                    Log.Debug("[SchemaProcessor] Creating Flag type {enumName}", flagObj.Name);
                    var eb = Global.DynamicModule.DefineEnum($"KzA.HEXEH.Core.Dynamic.Enums.{flagObj.Name}", TypeAttributes.Public, typeof(int));
                    var flagsAttrib = new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(Type.EmptyTypes)!, Array.Empty<object>());
                    eb.SetCustomAttribute(flagsAttrib);
                    foreach (var item in flagObj.Definition)
                    {
                        eb.DefineLiteral(item.Value, int.Parse(item.Key));
                    }
                    var flagsType = eb.CreateType();
                    Log.Debug("[SchemaProcessor] Created Flag type {enumName}", flagObj.Name);
                    types.Add(flagsType);
                    ParserManager.CreatedEnums.Add(flagObj.Name, flagsType);
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
                var typeName = ProcessSchemaFileName(schemaFile);
                if (typeName == null)
                {
                    Log.Warning("[SchemaProcessor] Invalid schema name {schemaFile}, skipping", schemaFile);
                    continue;
                }
                Log.Debug("[SchemaProcessor] Creating Parser for schema {schemaFile}, type name {typeName}", schemaFile, typeName);
                var tb = Global.DynamicModule.DefineType(
                        typeName,
                        TypeAttributes.Public | TypeAttributes.Class,
                        typeof(SchemaParser));
                _ = tb.CreateType();
                Log.Debug("[SchemaProcessor] Created Parser for schema {schemaFile}", schemaFile);
            }
            ParserManager.RefreshParsers();
        }

        private static string GenerateSchemaFilePath(Type ParserType)
        {
            var typeName = ParserType.FullName![30..];
            typeName = typeName.Remove(typeName.Length - 6);
            return typeName.Replace('.', Path.DirectorySeparatorChar) + ".json";
        }

        private static string? ProcessSchemaFileName(FileInfo schemaFile)
        {
            if (!TypeNameRegex().Match(schemaFile.Name).Success)
            {
                return null;
            }
            var relative = Path.GetRelativePath("./Schema", schemaFile.FullName);
            var typeName = relative.Remove(relative.Length - 5).Replace(Path.DirectorySeparatorChar, '.');
            var fullTypeName = "KzA.HEXEH.Core.Dynamic.Parser." + typeName + "Parser";
            return fullTypeName;
        }

        [GeneratedRegex(@"^[a-z0-9_]+\.json$", RegexOptions.IgnoreCase)]
        private static partial Regex TypeNameRegex();
    }
}
