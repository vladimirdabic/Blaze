using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
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
            // Console library
            var console_lib = new Library("console");
            env.DefineVariable("console", VariableType.PUBLIC, console_lib);

            console_lib.DefineFunction("print", (VM vm, List<IValue> args) =>
            {
                foreach (var arg in args)
                    Console.Write($"{arg.AsString()}");

                Console.Write('\n');
                return null;
            });

            console_lib.DefineFunction("input", (VM vm, List<IValue> args) =>
            {
                if (args.Count > 0)
                    Console.Write(args[0].AsString());

                string input = Console.ReadLine();
                return new StringValue(input);
            });

            console_lib.DefineFunction("clear", (VM vm, List<IValue> args) =>
            {
                Console.Clear();
                return null;
            });

            console_lib.DefineFunction("write", (VM vm, List<IValue> args) =>
            {
                foreach (var arg in args)
                    Console.Write($"{arg.AsString()}");

                return null;
            });

            console_lib.DefineFunction("key", (VM vm, List<IValue> args) =>
            {
                var k = Console.ReadKey();

                var res = new ListValue();
                res.Values.Add(new StringValue(k.KeyChar.ToString()));
                res.Values.Add(new NumberValue((double)k.Key));

                return res;
            });

            // Parse library
            var parse_lib = new Library("parse");
            env.DefineVariable("parse", VariableType.PUBLIC, parse_lib);

            parse_lib.DefineFunction("num", (VM vm, List<IValue> args) =>
            {
                if (args.Count == 0 || args[0] is not StringValue)
                    throw new InterpreterInternalException("Expected string value for function parse.num");

                var val = ((StringValue)args[0]).Value;
                double.TryParse(val, out double res);

                return new NumberValue(res);
            });

            parse_lib.DefineFunction("str", (VM vm, List<IValue> args) =>
            {
                if (args.Count == 0)
                    throw new InterpreterInternalException("Expected object value for function parse.str");

                return new StringValue(args[0].AsString());
            });

            parse_lib.DefineFunction("bool", (VM vm, List<IValue> args) =>
            {
                if (args.Count == 0)
                    throw new InterpreterInternalException("Expected object value for function parse.bool");

                return new BooleanValue(args[0].AsBoolean());
            });

            // Module library
            var module_lib = new Library("module");
            env.DefineVariable("module", VariableType.PUBLIC, module_lib);

            module_lib.DefineFunction("load", (VM vm, List<IValue> args) =>
            {
                if (args.Count == 0 || args[0] is not StringValue)
                    throw new InterpreterInternalException("Expected module name for function module.load");

                var mod_name = ((StringValue)args[0]).Value;

                Module module = new Module();
                MemoryStream stream = new MemoryStream(File.ReadAllBytes(mod_name));
                BinaryReader reader = new BinaryReader(stream);
                module.FromBinary(reader);

                // Load module file
                ModuleEnv env = VM.StaticLoadModule(module);

                // Set its parent to be the current running module
                env.SetParent(vm.Module);

                return new ModuleValue(env);
            });
        }
    }
}
