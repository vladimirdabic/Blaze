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
        // Might change in the future
        internal ModuleEnvironment _moduleEnv;

        public Stack<IValue> Stack;
        public static readonly NullValue NullInstance = new NullValue();

        public Interpreter()
        {
            Stack = new Stack<IValue>();
        }

        /// <summary>
        /// Loads a Module and returns a ModuleEnvironment
        /// </summary>
        /// <param name="module">Module to load</param>
        /// <param name="parent">Optional parent environment</param>
        /// <returns></returns>
        public ModuleEnvironment LoadModule(Module.Module module, ModuleEnvironment parent = null)
        {
            ModuleEnvironment env = new ModuleEnvironment(module, parent);

            // TODO: Error checking
            FunctionValue staticFunc = new FunctionValue(env.Module.Functions[0], null);

            RunFunction(env, staticFunc, null);
            return env;
        }

        /// <summary>
        /// Runs a function in a specific environment
        /// </summary>
        /// <param name="env">The environment</param>
        /// <param name="function">The function</param>
        /// <param name="args">List of arguments</param>
        /// <returns>Function return value</returns>
        public IValue RunFunction(ModuleEnvironment env, FunctionValue function, List<IValue> args)
        {
            _moduleEnv = env;
            Stack.Clear();

            try
            {
                return function.Call(this, args);
            } catch (InterpreterInternalException)
            {
                throw new InterpreterException(Stack.Pop());
            }
        }
    }
    public class InterpreterInternalException : Exception
    {
        public InterpreterInternalException()
        {
        }
    }

    public class InterpreterException : Exception
    {
        public IValue Value;

        public InterpreterException(IValue value)
        {
            Value = value;
        }
    }
}
