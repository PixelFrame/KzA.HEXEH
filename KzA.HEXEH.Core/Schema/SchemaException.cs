namespace KzA.HEXEH.Core.Schema
{
    internal class SchemaException(string Message, string SchemaName, string FieldName, Exception? InnerException = null) : Exception(Message, InnerException)
    {
        public string SchemaName { get; set; } = SchemaName;
        public string FieldName { get; set; } = FieldName;
    }
}
