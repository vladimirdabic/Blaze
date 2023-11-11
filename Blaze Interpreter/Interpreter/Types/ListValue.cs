using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class ListValue : IValue, IValueIndexable, IValueProperties, IValueIterable
    {
        public List<IValue> Values;
        public Dictionary<string, IValue> Properties;

        public ListValue()
        {
            Values = new List<IValue>();
            Properties = new Dictionary<string, IValue>();

            DefineProperties();
        }

        public bool AsBoolean()
        {
            // probably return true all the time, empty list being treated as false might not be the best thing
            return Values.Count != 0;
        }

        public string AsString()
        {
            List<string> reprs = new List<string>();

            foreach (var v in Values)
                reprs.Add(v.AsString());

            return $"[{string.Join(", ", reprs)}]";
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            // maybe compare each element, probably not though
            return other == this;
        }

        public string GetName()
        {
            return "list";
        }

        public IValue GetAtIndex(IValue index)
        {
            if (index is NumberValue numberValue)
            {
                int idx = Convert.ToInt32(numberValue.Value);

                // out of bounds
                if (idx < 0 || idx > Values.Count - 1) throw new IndexOutOfBounds();

                return Values[idx];
            }

            throw new IndexNotFound();
        }

        public void SetAtIndex(IValue index, IValue value)
        {
            if (index is NumberValue numberValue)
            {
                int idx = Convert.ToInt32(numberValue.Value);

                // out of bounds
                if (idx < 0 || idx > Values.Count - 1) throw new IndexOutOfBounds();

                Values[idx] = value;
                return;
            }

            throw new IndexNotFound();
        }

        private void DefineProperties()
        {
            Properties["append"] = new BuiltinFunctionValue("list.append", (Interpreter itp, List<IValue> args) =>
            {
                foreach(IValue value in args)
                    Values.Add(value);

                return this;
            });

            Properties["pop"] = new BuiltinFunctionValue("list.pop", (Interpreter itp, List<IValue> args) =>
            {
                if (args.Count != 1)
                {
                    itp.Stack.Push(new StringValue("Expected number parameter for function list.pop"));
                    throw new InterpreterInternalException();
                }

                IValue arg = args[0];

                if(arg is NumberValue numberValue)
                {
                    int idx = Convert.ToInt32(numberValue.Value);

                    // out of bounds
                    if (idx < 0 || idx > Values.Count - 1)
                    {
                        itp.Stack.Push(new StringValue("Index out of bounds for function list.pop"));
                        throw new InterpreterInternalException();
                    }

                    IValue value = Values[idx];
                    Values.RemoveAt(idx);
                    return value;
                }


                itp.Stack.Push(new StringValue("Expected number parameter for function list.pop"));
                throw new InterpreterInternalException();
            });
        }

        public IValue GetProperty(string name)
        {
            switch (name)
            {
                case "length":
                    return new NumberValue(Values.Count);

                default:
                    if (Properties.ContainsKey(name)) return Properties[name];
                    break; // will go to the throw
            }

            throw new PropertyNotFound();
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }

        public IteratorValue GetIterator()
        {
            return new ListIterator(this);
        }
    }
}
