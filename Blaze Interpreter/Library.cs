using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VD.Blaze.Interpreter;
using VD.Blaze.Interpreter.Types;

namespace Blaze_Interpreter
{
    public class Library : IValue, IValueProperties
    {
        public string Name;
        public Dictionary<string, IValue> Properties;


        public Library(string name)
        {
            Name = name;
            Properties = new Dictionary<string, IValue>();
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return $"<library '{Name}'>";
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
            return "library";
        }

        public IValue GetProperty(string name)
        {
            if(Properties.ContainsKey(name))
                return Properties[name];

            throw new PropertyNotFound();
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }


        public void DefineFunction(string name, Func<VM, List<IValue>, IValue> func)
        {
            var func_obj = new BuiltinFunctionValue(name, func);
            Properties[name] = func_obj;
        }

        public void Define(string name, IValue value)
        {
            Properties[name] = value;
        }
    }
}
