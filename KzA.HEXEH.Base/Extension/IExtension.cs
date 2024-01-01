namespace KzA.HEXEH.Base.Extension
{
    public interface IExtension
    {
        public string Name { get; }
        public IEnumerable<Type> Parsers { get; }
    }
}
