using KzA.HEXEH.Core.Extension;
using KzA.HEXEH.Core.Schema;
using Serilog;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("KzA.HEXEH.Test")]

namespace KzA.HEXEH.Core
{
    internal static class Global
    {
        internal static readonly int[] ValidLengthNumberLen = [1, 2, 4, /*8*/]; // ulong needs many conversion and is rarely used as a size
        internal static readonly int[] ValidNumberLen = [1, 2, 4, 8];
        internal static int LoopMax = 65535;
        internal static ModuleBuilder DynamicModule;
        internal static bool IsInitialized = false;

        static Global()
        {
            Log.Information(@"[Global] HEXEH Initializing
 __  __     ______     __  __     ______     __  __    
/\ \_\ \   /\  ___\   /\_\_\_\   /\  ___\   /\ \_\ \   
\ \  __ \  \ \  __\   \/_/\_\/_  \ \  __\   \ \  __ \  
 \ \_\ \_\  \ \_____\   /\_\/\_\  \ \_____\  \ \_\ \_\ 
  \/_/\/_/   \/_____/   \/_/\/_/   \/_____/   \/_/\/_/ 
                                                       
");
            Log.Debug("[Global] Creating dynamic assembly and module");
            var aName = new AssemblyName("KzA.HEXEH.Core.Dynamic");
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);
            DynamicModule = ab.DefineDynamicModule("KzA.HEXEH.Core.Dynamic");
            Log.Debug("[Global] Created dynamic assembly and module");

            Log.Debug("[Global] Creating dynamic parsers");
            SchemaProcessor.InitializeSchemaParsers();
            Log.Debug("[Global] Created dynamic parsers");

            Log.Debug("[Global] Loading extensions");
            ExtensionManager.LoadExtensions();
            Log.Debug("[Global] Loaded extensions");

            Log.Information("[Global] HEXEH Initialized");
            IsInitialized = true;
        }

        public static void Configure()
        {
        }

        public static void Initialize() { /* Nothing to be done here as we will do stuff in constructor */ }
    }
}
