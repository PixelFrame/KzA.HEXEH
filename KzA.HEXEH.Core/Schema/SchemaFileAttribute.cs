using KzA.HEXEH.Base.FileAccess;

namespace KzA.HEXEH.Core.Schema
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class SchemaFileAttribute : Attribute
    {
        public SchemaFile File { get; }
        public SchemaFileAttribute(string Root, string Name, string RelativePath)
        {
            File = new()
            {
                Root = Root,
                Name = Name,
                RelativePath = RelativePath
            };
        }
    }
}
