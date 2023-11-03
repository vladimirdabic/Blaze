using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

            add_func.Emit(Instruction.LDARG, 0);
            add_func.Emit(Instruction.LDARG, 1);
            add_func.Emit(Instruction.ADD);
            add_func.Emit(Instruction.RET);

            Function var_test = module.CreateFunction("var_test", 0);
            Variable my_var = module.DefineVariable("test", VariableType.PRIVATE);

            /*
             * private var test;
             * 
             * func var_test() {
             *     test = 0;
             * }
             */

            var_test.Emit(Instruction.LDCONST, 0);
            var_test.Emit(Instruction.STVAR, my_var);
            var_test.Emit(Instruction.LDVAR, my_var);
            var_test.Emit(Instruction.RET);


            Function local_test = module.CreateFunction("local_test", 2);

            /*
             * func local_test(x, y) {
             *     var a = 2 * x;
             *     var b = y + 3 * 2;
             *     return a + b;
             * }
             */

            /*
             * LDCONST 0    (2)
             * LDARG 0      (x)
             * MUL
             * STLOCAL 0    (a)
             * LDCONST 1    (3)
             * LDCONST 2    (2)
             * MUL
             * LDARG 1      (y)
             * ADD
             * STLOCAL 1    (b)
             * LDLOCAL 0    (a)
             * LDLOCAL 1    (b)
             * ADD
             * RET
             */


            LocalVariable lcl_a = local_test.DeclareLocal();
            LocalVariable lcl_b = local_test.DeclareLocal();

            // var a = 2 * x;
            local_test.Emit(Instruction.LDCONST, 0);
            local_test.Emit(Instruction.LDARG, 0);
            local_test.Emit(Instruction.MUL);
            local_test.Emit(Instruction.STLOCAL, lcl_a);
            // var b = y + 3 * 2;
            local_test.Emit(Instruction.LDCONST, 1);
            local_test.Emit(Instruction.LDCONST, 2);
            local_test.Emit(Instruction.MUL);
            local_test.Emit(Instruction.LDARG, 1);
            local_test.Emit(Instruction.ADD);
            local_test.Emit(Instruction.STLOCAL, lcl_b);
            // return a + b;
            local_test.Emit(Instruction.LDLOCAL, lcl_a);
            local_test.Emit(Instruction.LDLOCAL, lcl_b);
            local_test.Emit(Instruction.ADD);
            local_test.Emit(Instruction.RET);


            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            module.ToBinary(writer);

            File.WriteAllBytes("test.blzm", stream.ToArray());
        }
    }
}
