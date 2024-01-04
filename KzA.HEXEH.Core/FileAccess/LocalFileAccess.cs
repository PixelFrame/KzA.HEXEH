using KzA.HEXEH.Base.FileAccess;
using Serilog;

namespace KzA.HEXEH.Core.FileAccess
{
    internal class LocalFileAccess : IFileAccess
    {

        internal static readonly List<string> SchemaLocation = [
            "./Schema",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HEXEH/Schema")
            ];
        internal static readonly List<string> ExtensionLocation = [
            "./Extension",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HEXEH/Extension")
            ];

        public IEnumerable<DirectoryInfo> EnumExtensionDirs()
        {
            try
            {
                var exts = new List<DirectoryInfo>();
                foreach (var location in ExtensionLocation)
                {
                    var dir = new DirectoryInfo(location);
                    if (dir.Exists)
                    {
                        Log.Information("[LocalFileAccess] Loading extensions from {location}", location);
                        foreach (var d in dir.EnumerateDirectories())
                        {
                            Log.Debug("[LocalFileAccess] Found extension {extdir}", d.FullName);
                            exts.Add(d);
                        }
                    }
                }
                return exts;
            }
            catch (Exception e)
            {
                throw new IOException("Failure during enumerating extensions", e);
            }
        }

        public IEnumerable<SchemaFile> EnumSchemas()
        {
            try
            {
                var schemas = new List<SchemaFile>();
                foreach (var location in SchemaLocation)
                {
                    var dir = new DirectoryInfo(location);
                    if (dir.Exists)
                    {
                        Log.Information("[LocalFileAccess] Loading schemas from {location}", location);
                        foreach (var f in dir.EnumerateFiles("*.json", SearchOption.AllDirectories))
                        {
                            Log.Debug("[LocalFileAccess] Found schema {schema}", f.FullName);
                            schemas.Add(new SchemaFile
                            {
                                Name = f.Name,
                                Root = dir.FullName,
                                RelativePath = Path.GetRelativePath(dir.FullName, f.FullName),
                            });
                        }
                    }
                }
                return schemas;
            }
            catch (Exception e)
            {
                throw new IOException("Failure during enumerating schemas", e);
            }
        }

        public string ReadSchemaContent(SchemaFile schema)
        {
            Log.Debug("[LocalFileAccess] Reading file {schema}", schema.FullPath);
            return File.ReadAllText(schema.FullPath);
        }
    }
}
