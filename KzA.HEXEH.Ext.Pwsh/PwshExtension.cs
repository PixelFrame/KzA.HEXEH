using KzA.HEXEH.Base.Extension;

namespace KzA.HEXEH.Ext.Pwsh
{
    public class PwshExtension : IExtension
    {
        public string Name => "HEXEH PowerShell Extension";

        public IEnumerable<Type> Parsers => [typeof(PwshParser)];
    }
}
