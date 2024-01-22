using KzA.HEXEH.Base.FileAccess;
using KzA.HEXEH.Core.Extension;
using KzA.HEXEH.Core.FileAccess;
using KzA.HEXEH.Core.Parser;
using KzA.HEXEH.Core.Schema;
using Serilog;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KzA.HEXEH.Test")]

namespace KzA.HEXEH.Core
{
    public static class Global
    {
        internal static readonly int[] ValidLengthNumberLen = [1, 2, 4, /*8*/]; // ulong needs many conversion and is rarely used as a size
        internal static readonly int[] ValidNumberLen = [1, 2, 4, 8];
        internal static int LoopMax = 65535;
        internal static ModuleBuilder DynamicModule;
        internal static bool IsInitialized = false;
        internal static IFileAccess FileAccessor = new LocalFileAccess();

        static Global()
        {
            Log.Information(@"[Global] HEXEH START
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
        }

        public static void Configure(IFileAccess fileAccess)
        {
            FileAccessor = fileAccess;
        }

        public static void Initialize()
        {
            if (IsInitialized) return;
            if (FileAccessor.UseAsync) throw new Exception("File Accessor is set to async, call InitializeAsync instead");
            Log.Information("[Global] HEXEH Initializing parsers and extensions");

            Log.Debug("[Global] File Accessor is {accessorType}", FileAccessor.GetType().FullName);
            Log.Debug($"[Global] File accessing mode is sync");
            Log.Debug("[Global] Creating dynamic parsers");
            SchemaProcessor.InitializeSchemaParsers();
            Log.Debug("[Global] Created dynamic parsers");

            Log.Debug("[Global] Loading extensions");
            ExtensionManager.LoadExtensions();
            Log.Debug("[Global] Loaded extensions");

            Log.Information("[Global] HEXEH Initialized");
            IsInitialized = true;
            ParserManager.RefreshParsers();
        }

        public static async Task InitializeAsync()
        {
            if (IsInitialized) return;
            if (!FileAccessor.UseAsync) throw new Exception("File Accessor is set to sync, call Initialize instead");
            Log.Information("[Global] HEXEH Initializing parsers and extensions");

            Log.Debug("[Global] File Accessor is {accessorType}", FileAccessor.GetType().FullName);
            Log.Debug($"[Global] File accessing mode is async");
            Log.Debug("[Global] Creating dynamic parsers");
            await SchemaProcessor.InitializeSchemaParsersAsync();
            Log.Debug("[Global] Created dynamic parsers");

            Log.Debug("[Global] Loading extensions");
            await ExtensionManager.LoadExtensionsAsync();
            Log.Debug("[Global] Loaded extensions");

            Log.Information("[Global] HEXEH Initialized");
            IsInitialized = true;
            ParserManager.RefreshParsers();
        }
    }
}
