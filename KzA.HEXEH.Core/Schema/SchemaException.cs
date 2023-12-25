namespace KzA.HEXEH.Core.Schema
{
    internal class SchemaException : Exception
    {
        public string SchemaName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public SchemaException(string Message, string SchemaName, string FieldName)
            : base(Message)
        {
            this.SchemaName = SchemaName;
            this.FieldName = FieldName;
        }
    }
}
