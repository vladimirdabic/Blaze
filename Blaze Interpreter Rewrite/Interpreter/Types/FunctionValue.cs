using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter.Types
{
    public class FunctionValue : IValue, IValueCallable
    {
        public string Name = null;
        public int NumOfArgs;
        public bool Varargs;
        public int NumOfLocals { get; private set; }
        public List<Instruction> Instructions;
        public BaseEnv Closure;

        public FunctionValue(Function func, BaseEnv env)
        {
            NumOfArgs = func.NumOfArgs;
            Varargs = func.Varargs;
            NumOfLocals = func.NumOfLocals;
            Instructions = func.Instructions;
            Closure = env;

            if(func.Name is not null)
                Name = ((Constant.String)func.Name).Value;
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            // Function can only be equal if it's the same reference
            return other == this;
        }

        public string GetName()
        {
            return "function";
        }

        public void Call(Interpreter interpreter, List<IValue> args)
        {
            var env = new FuncEnv(Closure)
            {
                Arguments = args ?? new List<IValue>(),
                Locals = new IValue[NumOfLocals]
            };

            for (int i = 0; i < NumOfLocals; i++)
                env.Locals[i] = Interpreter.NullInstance;

            // Setup the context
            // interpreter.PushContext(Instructions);
            interpreter._instructions = Instructions;
            interpreter.Environment = env;
        }

        public string AsString()
        {
            return $"<function {Name ?? "<anonymous>"}>";
        }

        public bool AsBoolean()
        {
            return true;
        }
    }
}
