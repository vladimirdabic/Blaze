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
            ModuleEnvironment env = interpreter.LoadModule(module);

            var func = env.GetFunction("test");
            IValue ret = interpreter.RunFunction(env, func, null);

            Console.WriteLine("Running function test: ");
            Console.WriteLine(ret.AsString());
            Console.ReadKey();
        }


        static void Main2(string[] args)
        {
            Module module = new Module();

            Function add_func = module.CreateFunction("add", 2);

            /*
             * func add(x, y) {
             *     return x + y;
             * }
             */

            /*
             * LDARG 0 
             * LDARG 1
             * ADD
             * RET
             */

            add_func.Emit(Opcode.LDARG, 0);
            add_func.Emit(Opcode.LDARG, 1);
            add_func.Emit(Opcode.ADD);
            add_func.Emit(Opcode.RET);

            Function test_func = module.CreateFunction("test", 0);

            /*
             * func test() {
             *     return 20 + 12;
             * }
             */

            /*
             * LDCONST 
             * LDCONST 
             * ADD
             * RET
             */

            var n1 = module.AddConstant(new Constant.Number(20));
            var n2 = module.AddConstant(new Constant.Number(12));

            test_func.Emit(Opcode.LDCONST, n1);
            test_func.Emit(Opcode.LDCONST, n2);
            test_func.Emit(Opcode.ADD);
            test_func.Emit(Opcode.RET);

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            module.ToBinary(writer);

            File.WriteAllBytes("test.blzm", stream.ToArray());
        }
    }
}
