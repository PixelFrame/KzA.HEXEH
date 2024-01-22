using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.HEXEH.Base.FileAccess
{
    public interface IFileAccess
    {
        public bool UseAsync { get; set; }
        public IEnumerable<SchemaFile> EnumSchemas();
        public Task<IEnumerable<SchemaFile>> EnumSchemasAsync();
        public string ReadSchemaContent(SchemaFile schema);
        public Task<Stream> GetSchemaReadStreamAsync(SchemaFile schema);
        public IEnumerable<DirectoryInfo> EnumExtensionDirs();
        public Task<IEnumerable<DirectoryInfo>> EnumExtensionDirsAsync();
    }
}
