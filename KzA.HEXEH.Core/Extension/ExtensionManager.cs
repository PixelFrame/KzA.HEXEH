using KzA.HEXEH.Base.Extension;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Parser;
using Serilog;
using System.Reflection;

namespace KzA.HEXEH.Core.Extension
{
    public static class ExtensionManager
    {
        internal static void LoadExtensions()
        {
            var extdirs = Global.FileAccessor.EnumExtensionDirs();
            Log.Debug("[ExtensionManager] Found {count} extension dir(s)", extdirs.Count());
            foreach (var extdir in extdirs)
            {
                Log.Debug("[ExtensionManager] Start loading extension from {extdir}", extdir);
                var extdll = extdir.FullName + Path.DirectorySeparatorChar + extdir.Name + ".dll";
                var context = new ExtensionLoadContext(extdll);
                var assembly = context.LoadFromAssemblyName(AssemblyName.GetAssemblyName(extdll));
                var extType = assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IExtension))).FirstOrDefault();
                if (extType == null)
                {
                    Log.Debug("[ExtensionManager] Extension not found in {extdll}", extdll);
                    continue;
                }
                var extInfoObj = Activator.CreateInstance(extType);
                var types = extType.GetProperty("Parsers")!.GetValue(extInfoObj) as IEnumerable<Type>;
                foreach (var type in types!)
                {
                    if (type.IsAssignableTo(typeof(IParser)) && !type.IsInterface && !type.IsAbstract)
                    {
                        ParserManager.AvailableParsers.Add(type);
                        Log.Debug("[ExtensionManager] Extension parser added {parser}", type.FullName ?? type.Name);
                    }
                    else
                    {
                        Log.Debug("[ExtensionManager] Skipped unqualified type {parser}", type.FullName ?? type.Name);
                    }
                }
            }
        }
    }
}