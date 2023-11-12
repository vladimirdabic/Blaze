using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class BuiltinFunctionValue : IValue, IValueCallable
    {
        public string Name;
        public Func<Interpreter, List<IValue>, IValue> Callback;

        public BuiltinFunctionValue(string name, Func<Interpreter, List<IValue>, IValue> callback)
        {
            Name = name;
            Callback = callback;
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return $"<built-in function {Name}>";
        }

        public void Call(Interpreter interpreter, List<IValue> args)
        {
            var res = Callback(interpreter, args);

            interpreter.Stack.Push(res is null ? Interpreter.NullInstance : res);
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other == this;
        }

        public string GetName()
        {
            return "function";
        }
    }
}
