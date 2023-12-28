namespace KzA.HEXEH.Core.Parser
{
    internal class ParseException : Exception
    {
        public string ParserStackPrint { get; }
        public int Index { get; }
        public ParseException(string Message, string ParseStackPrint, int Index, Exception? InnerException)
            : base(Message, InnerException)
        {
            this.ParserStackPrint = ParseStackPrint;
            this.Index = Index;
        }
    }

    internal class ParseFailureException(string Message, string ParseStackPrint, int Index, Exception? InnerException) : ParseException(Message, ParseStackPrint, Index, InnerException)
    {
    }

    internal class ParseLengthMismatchException(string Message, string ParseStackPrint, int Index, Exception? InnerException) : ParseException(Message, ParseStackPrint, Index, InnerException)
    {
    }

    internal class ParseUnexpectedValueException(string Message, string ParseStackPrint, int Index) : ParseException(Message, ParseStackPrint, Index, null)
    {
    }
}
