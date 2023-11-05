using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public FuncEnvironment Closure;

        public FunctionValue(Function func, FuncEnvironment env)
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

        public IValue Call(Interpreter interpreter, List<IValue> args)
        {
            FuncEnvironment env = new FuncEnvironment(Closure);
            FuncEnvironment originalEnv = interpreter._env;

            env.Arguments = args ?? new List<IValue>();
            env.Locals = new IValue[NumOfLocals];

            for (int i = 0; i < NumOfLocals; i++)
                env.Locals[i] = Interpreter.NullInstance;

            interpreter._env = env;

            IValue ret = interpreter.Evaluate(Instructions);

            interpreter._env = originalEnv;
            return ret;
        }

        public string AsString()
        {
            return $"<function {Name ?? "<anonymous>"}>";
        }
    }
}
