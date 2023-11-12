using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public struct ExecutionContext
    {
        public int Current;
        public List<Instruction> Instructions;
        public BaseEnv Environment;
        public Stack<int> ExceptionStack;

        public ExecutionContext(int current, List<Instruction> instructions, BaseEnv environment, Stack<int> exceptionStack)
        {
            Current = current;
            Instructions = instructions;
            Environment = environment;
            ExceptionStack = exceptionStack;
        }
    }
}
