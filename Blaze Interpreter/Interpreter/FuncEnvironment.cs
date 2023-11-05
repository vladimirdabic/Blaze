using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;

namespace VD.Blaze.Interpreter
{
    public class FuncEnvironment
    {
        public FuncEnvironment Parent;
        public List<IValue> Arguments;
        public IValue[] Locals;
        public Stack<int> ExceptionStack;

        public FuncEnvironment(FuncEnvironment parent)
        {
            Parent = parent;
            Arguments = new List<IValue>();
            ExceptionStack = new Stack<int>();
        }
    }
}
