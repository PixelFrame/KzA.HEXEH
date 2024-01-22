using KzA.HEXEH.Base.FileAccess;
using KzA.HEXEH.Base.Parser;
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
        private static Dictionary<string, SchemaJsonObject> createdSchema = [];
        internal static Dictionary<string, SchemaJsonObject> CreatedSchema
        {
            get => createdSchema;
        }

        private static SchemaJsonObject? LoadSchemaPreCheck(Type ParserType, SchemaFile schemaFile)
        {
            if (!ParserType.IsAssignableTo(typeof(IParser)))
                throw new ArgumentException($"{ParserType.FullName} is not a schema parser");
            if (CreatedSchema.TryGetValue(schemaFile.FullPath, out SchemaJsonObject? value))
            {
                Log.Debug("[SchemaProcessor] SchemaJsonObject {schema} existing", schemaFile.FullPath);
                return value;
            }
            return null;
        }

        internal static SchemaJsonObject LoadSchema(Type ParserType)
        {
            var schemaFile = GetSchemaFromAttribute(ParserType);
            var precheck = LoadSchemaPreCheck(ParserType, schemaFile);
            if (null != precheck)
                return precheck;

            Log.Debug("[SchemaProcessor] Loading schema {schema}", schemaFile.FullPath);
            var schemaJsonContent = Global.FileAccessor.ReadSchemaContent(schemaFile);
            Log.Verbose($"[SchemaProcessor] Schema content:{Environment.NewLine}{{schemaJsonContent}}", schemaJsonContent);
            var schemaObj = JsonSerializer.Deserialize<SchemaJsonObject>(schemaJsonContent)!;
            CreatedSchema.Add(schemaFile.FullPath, schemaObj);
            Log.Debug("[SchemaProcessor] Loaded schema {schema}", schemaFile.FullPath);
            return schemaObj;
        }

        /// This method is not in use as we have pre-created all schema objects for async mode
        internal static async Task<SchemaJsonObject> LoadSchemaAsync(Type ParserType)
        {
            var schemaFile = GetSchemaFromAttribute(ParserType);
            var precheck = LoadSchemaPreCheck(ParserType, schemaFile);
            if (null != precheck)
                return precheck;

            Log.Debug("[SchemaProcessor] Loading schema {schema} async", schemaFile.FullPath);
            var schemaJsonStream = await Global.FileAccessor.GetSchemaReadStreamAsync(schemaFile);
            var schemaObj = (await JsonSerializer.DeserializeAsync<SchemaJsonObject>(schemaJsonStream))!;
            CreatedSchema.Add(schemaFile.FullPath, schemaObj);
            Log.Debug("[SchemaProcessor] Loaded schema {schema}", schemaFile.FullPath);
            await schemaJsonStream.DisposeAsync();

            return schemaObj;
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
                    var flagsAttrib = new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(Type.EmptyTypes)!, []);
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
            var schemaFiles = Global.FileAccessor.EnumSchemas();
            InitializeSchemaParsersActual(schemaFiles);
        }

        internal static async Task InitializeSchemaParsersAsync()
        {
            var schemaFiles = await Global.FileAccessor.EnumSchemasAsync();
            InitializeSchemaParsersActual(schemaFiles);

            // Create all schema objects in advance as we do not have plan to support async parse yet.
            foreach(var schemaFile in schemaFiles)
            {
                Log.Debug("[SchemaProcessor] Loading schema {schema} async", schemaFile.FullPath);
                var schemaJsonStream = await Global.FileAccessor.GetSchemaReadStreamAsync(schemaFile);
                var schemaObj = (await JsonSerializer.DeserializeAsync<SchemaJsonObject>(schemaJsonStream))!;
                CreatedSchema.Add(schemaFile.FullPath, schemaObj);
                Log.Debug("[SchemaProcessor] Loaded schema {schema}", schemaFile.FullPath);
                await schemaJsonStream.DisposeAsync();
            }
        }

        private static void InitializeSchemaParsersActual(IEnumerable<SchemaFile> schemaFiles)
        {
            foreach (var schemaFile in schemaFiles)
            {
                var typeName = ProcessSchemaFileName(schemaFile);
                if (typeName == null)
                {
                    Log.Warning("[SchemaProcessor] Invalid schema name {schemaFile}, skipping", schemaFile.FullPath);
                    continue;
                }
                Log.Debug("[SchemaProcessor] Creating Parser for schema {schemaFile}, type name {typeName}", schemaFile.FullPath, typeName);
                var tb = Global.DynamicModule.DefineType(
                        typeName,
                        TypeAttributes.Public | TypeAttributes.Class,
                        typeof(SchemaParser));
                var attribCtorParams = new Type[] { typeof(string), typeof(string), typeof(string), };
                var ctorInfo = typeof(SchemaFileAttribute).GetConstructor(attribCtorParams)!;
                var attribBuilder = new CustomAttributeBuilder(ctorInfo,
                               new object[] { schemaFile.Root, schemaFile.Name, schemaFile.RelativePath });
                tb.SetCustomAttribute(attribBuilder);
                _ = tb.CreateType();
                Log.Debug("[SchemaProcessor] Created Parser for schema {schemaFile}", schemaFile.FullPath);
            }
        }

        private static SchemaFile GetSchemaFromAttribute(Type ParserType)
        {
            // Somehow ParserType.GetCustomAttributes() always return an empty array...
            var file = new SchemaFile();
            var args = ParserType.CustomAttributes.First().ConstructorArguments.ToArray();
            file.Root = (string)args[0].Value!;
            file.Name = (string)args[1].Value!;
            file.RelativePath = (string)args[2].Value!;
            return file;
        }

        private static string? ProcessSchemaFileName(SchemaFile schemaFile)
        {
            if (!TypeNameRegex().Match(schemaFile.Name).Success)
            {
                return null;
            }
            var typeName = schemaFile.RelativePath.Remove(schemaFile.RelativePath.Length - 5).Replace(Path.DirectorySeparatorChar, '.');
            var fullTypeName = "KzA.HEXEH.Core.Dynamic.Parser." + typeName + "Parser";
            return fullTypeName;
        }

        [GeneratedRegex(@"^[a-z0-9_]+\.json$", RegexOptions.IgnoreCase)]
        private static partial Regex TypeNameRegex();
    }
}
