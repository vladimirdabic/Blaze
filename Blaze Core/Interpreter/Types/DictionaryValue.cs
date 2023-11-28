using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class DictionaryValue : IValue, IValueIndexable, IValueProperties, IValueIterable
    {
        public Dictionary<IValue, IValue> Entries;
        public Dictionary<string, IValue> Properties;

        public DictionaryValue()
        {
            Entries = new Dictionary<IValue, IValue>();
            Properties = new Dictionary<string, IValue>();

            DefineProperties();
        }

        public bool AsBoolean()
        {
            return true;
        }

        public string AsString()
        {
            return "<dict>";
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            return other == this;
        }

        public IValue GetAtIndex(IValue index)
        {
            return Entries.ContainsKey(index) ? Entries[index] : VM.NullInstance;
        }

        public IteratorValue GetIterator()
        {
            return new DictIterator(this);
        }

        public string GetName()
        {
            return "dict";
        }

        public IValue GetProperty(string name)
        {
            switch (name)
            {
                case "length":
                    return new NumberValue(Entries.Count);

                case "keys":
                    {
                        var list = new ListValue();

                        foreach(var key in Entries.Keys)
                            list.Values.Add(key);

                        return list;
                    }

                default:
                    if (Properties.ContainsKey(name)) return Properties[name];
                    break;
            }

            throw new PropertyNotFound();
        }

        public void SetAtIndex(IValue index, IValue value)
        {
            Entries[index] = value;
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }


        private void DefineProperties()
        {
            Properties["contains"] = new BuiltinFunctionValue("dict.contains", (VM itp, List<IValue> args) =>
            {
                if (args.Count != 1)
                {
                    throw new InterpreterInternalException("Expected object argument for function dict.contains");
                }

                return new BooleanValue(Entries.ContainsKey(args[0]));
            });

            Properties["get"] = new BuiltinFunctionValue("dict.get", (VM itp, List<IValue> args) =>
            {
                if (args.Count != 2)
                {
                    throw new InterpreterInternalException("Expected key and default object arguments for function dict.get");
                }

                return Entries.ContainsKey(args[0]) ? Entries[args[0]] : args[1];
            });
        }
    }
}
