using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VD.Blaze.Interpreter;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace Blaze_Interpreter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Module module = new Module();

            MemoryStream stream = new MemoryStream(File.ReadAllBytes("test.blzm"));
            BinaryReader reader = new BinaryReader(stream);

            module.FromBinary(reader);
            module.PrintToConsole();

            Console.Write('\n');
            Console.ReadKey();


            Interpreter interpreter = new Interpreter();
            ModuleEnvironment globalEnvironment = new ModuleEnvironment();
            SetupGlobals(globalEnvironment);
           
            // Load module file
            ModuleEnvironment env = interpreter.LoadModule(module);
            
            // Set the parent to be the global environment
            env.Parent = globalEnvironment;
            globalEnvironment.Children.Add(env);

            // Run main
            Console.WriteLine("Running function main: ");

            var func = env.GetFunction("main");
            IValue ret = interpreter.RunFunction(env, func, null);

            // Debug stuff
            //Console.WriteLine(ret.AsString());
            /*Console.WriteLine("\nStack: ");
            for(int i = 0; i < interpreter._stack.Count; ++i)
            {
                Console.WriteLine(interpreter._stack.Pop().AsString());
            }*/

            Console.ReadKey();
        }

        static void SetupGlobals(ModuleEnvironment env)
        {
            // Define print function
            var print_func = new BuiltinFunctionValue("print", (Interpreter itp, List<IValue> args) =>
            {
                foreach (var arg in args)
                    Console.Write($"{arg.AsString()} ");

                Console.Write('\n');
                return null;
            });

            env.DefineVariable("print", VariableType.PUBLIC, print_func);
        }
    }
}
