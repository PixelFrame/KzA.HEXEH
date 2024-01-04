using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.HEXEH.Base.FileAccess
{
    public interface IFileAccess
    {
        public IEnumerable<SchemaFile> EnumSchemas();
        public string ReadSchemaContent(SchemaFile schema);
        public IEnumerable<DirectoryInfo> EnumExtensionDirs();
    }
}
