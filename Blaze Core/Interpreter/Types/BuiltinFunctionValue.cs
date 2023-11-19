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
        public Func<VM, List<IValue>, IValue> Callback;

        public BuiltinFunctionValue(string name, Func<VM, List<IValue>, IValue> callback)
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

        public void Call(VM vm, List<IValue> args)
        {
            var res = Callback(vm, args);

            vm.Stack.Push(res is null ? VM.NullInstance : res);
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
