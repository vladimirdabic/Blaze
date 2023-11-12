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
    internal class Utils
    {
        public static void CreateLibraries(ModuleEnv env)
        {
            // Define global print function
            var print_func = new BuiltinFunctionValue("print", (Interpreter itp, List<IValue> args) =>
            {
                foreach (var arg in args)
                    Console.Write($"{arg.AsString()}");

                Console.Write('\n');
                return null;
            });

            env.DefineVariable("print", VariableType.PUBLIC, print_func);


            // Module library
            var module_lib = new Library("module");
            env.DefineVariable("module", VariableType.PUBLIC, module_lib);

            module_lib.DefineFunction("load", (Interpreter itp, List<IValue> args) =>
            {
                if (args.Count == 0 || args[0] is not StringValue)
                    throw new InterpreterInternalException("Expected module name for function module.load");

                var mod_name = ((StringValue)args[0]).Value;

                Module module = new Module();
                MemoryStream stream = new MemoryStream(File.ReadAllBytes(mod_name));
                BinaryReader reader = new BinaryReader(stream);
                module.FromBinary(reader);

                Interpreter interpreter = new Interpreter();

                // Load module file
                ModuleEnv env = interpreter.LoadModule(module);

                // Set its parent to be the current running module
                env.SetParent(itp.Module);

                // Return null for now
                return null;
            });
        }
    }
}
