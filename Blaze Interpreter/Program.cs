using Blaze_Interpreter.ArgParse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Lib;
using VD.Blaze.Module;
using BlzModule = VD.Blaze.Module.Module;


namespace Blaze_Interpreter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var argIt = new ArgumentIterator(args);

            string moduleFileName = null;
            bool dump = false;

            while(argIt.Available())
            {
                var arg = argIt.Next();
                switch(arg)
                {
                    case "-m":
                        moduleFileName = argIt.Next();

                        if(moduleFileName is null)
                        {
                            Console.WriteLine($"Expected module file name after '-m'");
                            return;
                        }

                        break;

                    case "-d":
                        dump = true;
                        break;

                    default:
                        Console.WriteLine($"Unknown argument '{arg}'");
                        return;
                }
            }

            if(moduleFileName is null)
            {
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} [options]");
                Console.WriteLine($"Options:");
                Console.WriteLine($"    -m [file]       Module file to execute");
                Console.WriteLine($"    -d              Print the contents of the module file");
                return;
            }

            if (!File.Exists(moduleFileName))
            {
                Console.WriteLine("Module file not found");
                return;
            }

            try {
                // Load module
                BlzModule module = new BlzModule();

                MemoryStream stream = new MemoryStream(File.ReadAllBytes(moduleFileName));
                BinaryReader reader = new BinaryReader(stream);

                module.FromBinary(reader);

                if (dump)
                {
                    module.PrintToConsole();
                    return;
                }

                // Setup vm and internal module
                VM vm = new VM();
                Executor exec = new Executor();

                ModuleEnv internal_module = new ModuleEnv(); // Might use in future

                // Load libraries
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string libDir = Path.Combine(exeDir, "lib");

                if (Directory.Exists(libDir))
                {
                    foreach (var file in Directory.GetFiles(libDir, "*.dll"))
                    {
                        var asm = Assembly.LoadFrom(file);
                        LibraryLoader.Load(asm, internal_module);
                    }
                }

                // Load user module
                ModuleEnv env = vm.LoadModule(module);
                env.SetParent(internal_module);

                // Run main
                var func = env.GetFunction("main");

                if(func is null)
                {
                    Console.WriteLine("Entry point 'main' not found");
                    return;
                }

                // vm.RunFunction(func, null);

                exec.LoadFunction(vm, func, null);
                exec.Execute();
            }
            catch (VMException e)
            {
                if (e.Location.line == 0)
                    Console.WriteLine($"[{e.Location.filename}] {e.Value.AsString()}");
                else
                    Console.WriteLine($"[{e.Location.filename}:{e.Location.line}] {e.Value.AsString()}");
            }
            catch(FileLoadException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
