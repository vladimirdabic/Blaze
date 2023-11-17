using Blaze_Interpreter.ArgParse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;


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

            Module module = new Module();

            MemoryStream stream = new MemoryStream(File.ReadAllBytes(moduleFileName));
            BinaryReader reader = new BinaryReader(stream);

            module.FromBinary(reader);

            if (dump)
            {
                module.PrintToConsole();
                return;
            }

            Interpreter interpreter = new Interpreter();
            ModuleEnv globalEnvironment = new ModuleEnv();
            Utils.CreateLibraries(globalEnvironment);

            try
            {
                ModuleEnv env = interpreter.LoadModule(module);

                // Set the parent to be the global environment
                env.SetParent(globalEnvironment);

                // Run main
                var func = env.GetFunction("main");

                if(func is null)
                {
                    Console.WriteLine("Entry point 'main' not found");
                    return;
                }

                interpreter.RunFunction(env, func, null);
            }
            catch (InterpreterException e)
            {
                if (e.Location.line == 0)
                    Console.WriteLine($"[{e.Location.filename}] {e.Value.AsString()}");
                else
                    Console.WriteLine($"[{e.Location.filename}:{e.Location.line}] {e.Value.AsString()}");
            }
        }
    }
}
