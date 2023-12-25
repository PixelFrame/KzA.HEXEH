using KzA.HEXEH.Core.Schema;
using Serilog;
using System.Reflection;
using System.Reflection.Emit;

namespace KzA.HEXEH.Core
{
    internal static class Global
    {
        internal static readonly int[] ValidLengthNumberLen = [1, 2, 4, /*8*/]; // ulong needs many conversion and is rarely used as a size
        internal static int LoopMax = 65535;
        internal static ModuleBuilder DynamicModule;
        internal static bool IsInitialized = false;

        static Global()
        {
            Log.Information(@"HEXEH Initializing
 __  __     ______     __  __     ______     __  __    
/\ \_\ \   /\  ___\   /\_\_\_\   /\  ___\   /\ \_\ \   
\ \  __ \  \ \  __\   \/_/\_\/_  \ \  __\   \ \  __ \  
 \ \_\ \_\  \ \_____\   /\_\/\_\  \ \_____\  \ \_\ \_\ 
  \/_/\/_/   \/_____/   \/_/\/_/   \/_____/   \/_/\/_/ 
                                                       
");
            Log.Verbose("Creating dynamic assembly and module");
            var aName = new AssemblyName("KzA.HEXEH.Core.Dynamic");
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);
            DynamicModule = ab.DefineDynamicModule("KzA.HEXEH.Core.Dynamic");
            Log.Verbose("Created dynamic assembly and module");

            Log.Verbose("Creating dynamic parsers");
            SchemaProcessor.InitializeSchemaParsers();
            Log.Verbose("Created dynamic parsers");

            Log.Information("HEXEH Initialized");
            IsInitialized = true;
        }

        public static void Configure()
        {
        }

        public static void Initialize() { /* Nothing to be done here as we will do stuff in constructor */ }
    }
}
