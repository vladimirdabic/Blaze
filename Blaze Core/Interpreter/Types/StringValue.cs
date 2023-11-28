using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Interpreter.Types
{
    public class StringValue : IValue, IValueBinOp, IValueProperties, IValueIndexable, IValueIterable
    {
        public string Value;
        public Dictionary<string, IValue> Properties;

        public StringValue(string value)
        {
            Value = value;
            Properties = new Dictionary<string, IValue>();

            DefineProperties();
        }

        public IValue Copy()
        {
            return this;
        }

        public bool Equals(IValue other)
        {
            if (other is StringValue otherStr) return Value == otherStr.Value;

            return false;
        }

        public override bool Equals(object obj)
        {
            if(obj is StringValue otherStr)
                return Value == otherStr.Value;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public string GetName()
        {
            return "string";
        }

        public string AsString()
        {
            return $"{Value}";
        }

        public IValue Add(IValue other)
        {
            if (other is NumberValue otherNum)
                // Add string and number
                return new StringValue(Value + otherNum.Value);

            if (other is StringValue otherStr)
                // Add strings
                return new StringValue(Value + otherStr.Value);

            return null;
        }

        public IValue Subtract(IValue other)
        {
            return null;
        }

        public IValue Multiply(IValue other)
        {
            if (other is NumberValue otherNum)
                return new StringValue(new StringBuilder(Value.Length * (int)otherNum.Value).Insert(0, Value, (int)otherNum.Value).ToString());

            return null;
        }

        public IValue Divide(IValue other)
        {
            return null;
        }

        public IValue LessThan(IValue other)
        {
            return null;
        }

        public IValue LessThanEquals(IValue other)
        {
            return null;
        }

        public bool AsBoolean()
        {
            return true;
        }

        public IValue GetAtIndex(IValue index)
        {
            if (index is NumberValue numberValue)
            {
                int idx = Convert.ToInt32(numberValue.Value);

                // out of bounds
                if (idx < 0 || idx > Value.Length - 1) throw new IndexOutOfBounds();

                return new StringValue(Value[idx].ToString());
            }

            throw new IndexNotFound();
        }

        public void SetAtIndex(IValue index, IValue value)
        {
            throw new NotImplementedException();
        }

        public IValue GetProperty(string name)
        {
            switch(name)
            {
                case "length":
                    return new NumberValue(Value.Length);

                default:
                    if (Properties.ContainsKey(name)) return Properties[name];
                    break;
            }

            throw new PropertyNotFound();
        }

        public void SetProperty(string name, IValue value)
        {
            throw new PropertyNotFound();
        }

        private void DefineProperties()
        {
            Properties["split"] = new BuiltinFunctionValue("string.split", (VM itp, List<IValue> args) =>
            {
                if (args.Count != 1)
                {
                    throw new InterpreterInternalException("Expected delimiter parameter for function string.split");
                }

                IValue arg = args[0];

                if (arg is StringValue stringValue)
                {
                    string delimiter = stringValue.Value;
                    var parts = Value.Split(new string[] { delimiter }, StringSplitOptions.None);

                    var res = new ListValue();
                    foreach (var part in parts)
                        res.Values.Add(new StringValue(part));

                    return res;
                }

                throw new InterpreterInternalException("Expected string parameter for delimiter in function string.split");
            });
        }

        public IteratorValue GetIterator()
        {
            return new StringIterator(Value);
        }
    }
}