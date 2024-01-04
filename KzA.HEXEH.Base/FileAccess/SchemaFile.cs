using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.HEXEH.Base.FileAccess
{
    public class SchemaFile
    {
        public string Root { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string FullPath => $"{Root}/{RelativePath}";
    }
}
