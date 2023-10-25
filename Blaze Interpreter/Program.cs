using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Module;

namespace Blaze_Interpreter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Module program = new Module();
            
            // Probably gonna remove the number of arguments as the CALL instruction will pass the argument count
            Function main_func = program.CreateFunction("main", 1);

            // IDEAS
            /*main_func.Emit(Instruction.Ldnum, Const_idx);
            main_func.Emit(Instruction.Ldarg, idx);
            int local_idx = main_func.DeclareLocal();

            main_func.ToBinary();

            Function.FromBinary();*/
        }
    }
}
