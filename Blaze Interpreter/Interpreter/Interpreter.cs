using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Module;

namespace VD.Blaze.Interpreter
{
    public class Interpreter
    {
        // We'll see if this is how this will look like
        internal ModuleEnvironment _moduleEnv;
        // internal FuncEnvironment _env;

        internal Stack<IValue> _stack;
        public static readonly NullValue NullInstance = new NullValue();

        public Interpreter()
        {
            _stack = new Stack<IValue>();
        }

        public ModuleEnvironment LoadModule(Module.Module module)
        {
            ModuleEnvironment env = new ModuleEnvironment(module);

            // TODO: Error checking
            FunctionValue staticFunc = new FunctionValue(env.Module.Functions[0], null);

            RunFunction(env, staticFunc, null);
            
            return env;
        }


        public IValue RunFunction(ModuleEnvironment env, FunctionValue function, List<IValue> args)
        {
            _moduleEnv = env;
            _stack.Clear();

            return function.Call(this, args);
        }
    }
    public class InterpreterInternalException : Exception
    {
        public InterpreterInternalException()
        {
        }
    }
}
